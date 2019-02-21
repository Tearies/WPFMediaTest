using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace WPFMediaTest
{
    public class MediaControl : UserControl
    {
        #region Fields
        protected Border _Border;
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
                _mediaPlayer.MediaOpened += _mediaPlayer_MediaOpened;
                videoDrawing.Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
                videoDrawing.Player = _mediaPlayer;
                _Border = new Border();
                brush = new DrawingBrush(videoDrawing);
                brush.Stretch = Stretch.Fill;
                RenderOptions.SetBitmapScalingMode(_Border, BitmapScalingMode.NearestNeighbor);
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
           // videoDrawing.Rect = new Rect(0, 0, videoDrawing.Player.NaturalVideoWidth, videoDrawing.Player.NaturalVideoHeight);
        }

        private void Begin()
        {
            CurrentMediaClock?.Controller?.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0L), TimeSeekOrigin.BeginTime);
        }







        #endregion

        #region other

        private Border VideoBorder
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
            brush.Stretch = stretch;
            CurrentMediaClock?.Controller?.Begin();
        }
    }
}