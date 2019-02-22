using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MS.Internal;

namespace WPFMediaTest
{
    public class MediaControl : UserControl
    {
        #region Fields
        protected Canvas _Border;
        protected MediaPlayer _mediaPlayer;

        private bool isPlayed;
        private long OneSencondToMis = 50;

        #endregion

        #region Constructors

        public MediaControl()
        {

        }

        #endregion

        #region Properties



        #endregion

        #region Implementations

        private string fileName;
        private DrawingBrush brush;
        private VideoDrawing videoDrawing;
        public void Start()
        {
            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer();
                videoDrawing = new VideoDrawing();
                brush = new DrawingBrush(videoDrawing);
                _mediaPlayer.MediaOpened += _mediaPlayer_MediaOpened;
                //videoDrawing.Rect = new Rect(new Point(), MeasureArrangeHelper(this.RenderSize));
                videoDrawing.Player = _mediaPlayer;
                _Border = new Canvas();
                brush.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(_Border, BitmapScalingMode.Fant);
                _Border.UseLayoutRounding = true;
                Content = VideoBorder;
            }

            var open = new OpenFileDialog();
            var r = open.ShowDialog();
            if (!r.HasValue || !r.Value)
            {
                return;
            }
            fileName = open.FileName;
            CreateMediaClock();
            _mediaPlayer.Clock = CurrentMediaClock;
            if (CurrentMediaClock != null && CurrentMediaClock.Controller != null)
            {

                if (CurrentMediaClock.IsPaused)
                {
                    CurrentMediaClock.Controller.Resume();
                    if (!isPlayed)
                        Begin();
                }
                else
                {
                    Begin();
                }
                isPlayed = true;

            }
        }

        private void _mediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            videoDrawing.Rect = new Rect(new Point(), MeasureArrangeHelper(this.RenderSize));
        }

        private void Begin()
        {
            CurrentMediaClock?.Controller?.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0L), TimeSeekOrigin.BeginTime);
        }







        #endregion

        #region other

        private Canvas VideoBorder
        {
            get
            {
                if (_Border != null && _Border.Parent != null)
                    _Border.Disconnect(_Border.Parent);
                return _Border;
            }
        }




        private MediaClock _currentMediaClock;

        private MediaClock CurrentMediaClock
        {
            get => _currentMediaClock;
            set
            {
                if (_currentMediaClock != null) _currentMediaClock.CurrentTimeInvalidated -= Clock_CurrentTimeInvalidated;

                _currentMediaClock = value;
                _currentMediaClock.CurrentTimeInvalidated += Clock_CurrentTimeInvalidated;
            }
        }

        private void CreateMediaClock()
        {
            CurrentMediaClock = new MediaTimeline(new Uri(fileName, UriKind.RelativeOrAbsolute)).CreateClock();
        }


        private void Clock_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            if (CurrentMediaClock == null)
                return;
            if (CurrentMediaClock.NaturalDuration == Duration.Automatic)
                return;
            if (!CurrentMediaClock.CurrentTime.HasValue)
                return;
            if (CurrentMediaClock.Controller == null) return;
            if (CurrentMediaClock.CurrentTime.Value.TotalMilliseconds < OneSencondToMis)
            {
                SetBlackFrame();
            }
            else
            {
                _Border.Background = brush;
            }

        }

        private void SetBlackFrame()
        {
            _Border.Background = Brushes.Black;
        }


        #endregion

        public void SetStrech(Stretch stretch)
        {

            CurrentMediaClock?.Controller?.Stop();
            videoDrawing.Rect = new Rect(new Point(), MeasureArrangeHelper(this.RenderSize));
            brush.Stretch = stretch;
            CurrentMediaClock?.Controller?.Begin();
        }

        private Size MeasureArrangeHelper(Size inputSize)
        {
            MediaPlayer player = _mediaPlayer;
            if (player == null)
                return new Size();
            Size contentSize = new Size((double)player.NaturalVideoWidth, (double)player.NaturalVideoHeight);
            Size scaleFactor = ComputeScaleFactor(inputSize, contentSize, brush.Stretch, StretchDirection.Both);
            return new Size(contentSize.Width * scaleFactor.Width, contentSize.Height * scaleFactor.Height);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (CurrentMediaClock != null && _mediaPlayer.HasVideo && _mediaPlayer.Clock!=null)
            {
                CurrentMediaClock?.Controller?.Stop();
                videoDrawing.Rect = new Rect(new Point(), MeasureArrangeHelper(this.RenderSize));
                CurrentMediaClock?.Controller?.Begin();
            }

        }

        internal static Size ComputeScaleFactor(
            Size availableSize,
            Size contentSize,
            Stretch stretch,
            StretchDirection stretchDirection)
        {
            double width = 1.0;
            double height = 1.0;
            bool flag1 = !double.IsPositiveInfinity(availableSize.Width);
            bool flag2 = !double.IsPositiveInfinity(availableSize.Height);
            if ((stretch == Stretch.Uniform || stretch == Stretch.UniformToFill || stretch == Stretch.Fill) && flag1 | flag2)
            {
                width = DoubleUtil.IsZero(contentSize.Width) ? 0.0 : availableSize.Width / contentSize.Width;
                height = DoubleUtil.IsZero(contentSize.Height) ? 0.0 : availableSize.Height / contentSize.Height;
                if (!flag1)
                    width = height;
                else if (!flag2)
                {
                    height = width;
                }
                else
                {
                    switch (stretch)
                    {
                        case Stretch.Uniform:
                            double num1 = width < height ? width : height;
                            width = height = num1;
                            break;
                        case Stretch.UniformToFill:
                            double num2 = width > height ? width : height;
                            width = height = num2;
                            break;
                    }
                }
                switch (stretchDirection)
                {
                    case StretchDirection.UpOnly:
                        if (width < 1.0)
                            width = 1.0;
                        if (height < 1.0)
                        {
                            height = 1.0;
                            break;
                        }
                        break;
                    case StretchDirection.DownOnly:
                        if (width > 1.0)
                            width = 1.0;
                        if (height > 1.0)
                        {
                            height = 1.0;
                            break;
                        }
                        break;
                }
            }
            return new Size(width, height);
        }
    }

    public static class DoubleUtil
    {
        public static bool IsZero(double value)
        {
            return Math.Abs(value) < 2.22044604925031E-15;
        }
    }


}