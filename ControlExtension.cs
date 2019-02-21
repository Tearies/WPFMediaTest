using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace WPFMediaTest
{
    public static class ControlExtension
    {
        #region BindCommand

        /// <summary>
        /// 绑定命令和命令事件到宿主UI
        /// </summary>
        public static void BindCommand(this UIElement @ui, ICommand com, Action<object, ExecutedRoutedEventArgs> call)
        {
            var bind = new CommandBinding(com);
            bind.Executed += new ExecutedRoutedEventHandler(call);
            @ui.CommandBindings.Add(bind);
        }

        /// <summary>
        /// 绑定RelayCommand命令到宿主UI
        /// </summary>
        public static void BindCommand(this UIElement @ui, ICommand com)
        {
            var bind = new CommandBinding(com);
            @ui.CommandBindings.Add(bind);
        }

        #endregion

        #region  Storyboard





        /// <summary>
        /// 释放动画资源，containingObject为Storyboard关联的容器
        /// </summary>
        /// <param name="this"></param>
        /// <param name="containingObject"></param>
        public static void Dispose(this Storyboard @this, FrameworkElement containingObject = null)
        {
            if (@this == null)
                return;
            @this.Stop();
            if (containingObject == null)
                @this.Remove();
            else
                @this.Remove(containingObject);
            @this.Children.Clear();
        }

        #endregion

        #region FindParent，FindChildren
        /// <summary>
        /// 获取指定条件的父元素并转换为指定泛型参数的类型，若没找到，则返回null.
        /// </summary>
        public static T FindParent<T>(this DependencyObject obj, Func<DependencyObject, bool> predicate) where T : FrameworkElement
        {
            if (!(obj is Visual) && !(obj is Visual3D))
                return null;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T && predicate(parent))
                    return parent as T;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// 获取指定条件的父元素并转换为指定泛型参数的类型，若没找到，则返回null.
        /// </summary>
        public static T FindFirstParent<T>(this DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// 查找指定条件的子控件，找到第一个就返回，如果没有找到，则返回null
        /// </summary>
        public static T FindChild<T>(this DependencyObject obj, Func<DependencyObject, bool> predicate) where T : FrameworkElement
        {
            if (obj == null) return null;
            T t = null;
            DependencyObject child = null;
            int count = VisualTreeHelper.GetChildrenCount(obj);
            //尝试从content获取
            if (count == 0 && obj is ContentControl)
            {
                var objc = obj as ContentControl;
                t = DoPredicate<T>(objc.Content, predicate);
            }
            if (count == 0 && obj is ContentPresenter)
            {
                var objc = obj as ContentPresenter;
                t = DoPredicate<T>(objc.Content, predicate);
            }
            if (t != null) return t;

            for (int i = 0; i < count; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                t = DoPredicate<T>(child, predicate);
                if (t != null) return t;
            }
            return null;
        }

        private static T DoPredicate<T>(object obj, Func<DependencyObject, bool> predicate) where T : FrameworkElement
        {
            if (obj == null) return null;
            var cc = obj as DependencyObject;
            if (cc == null) return null;
            if (cc is T && predicate(cc))
                return cc as T;
            return cc.FindChild<T>(predicate);
        }

        /// <summary>
        /// 查找指定条件的所有子控件集合，没找到则返回空元素
        /// </summary>
        public static List<T> FindChildren<T>(this DependencyObject obj, Func<DependencyObject, bool> predicate) where T : FrameworkElement
        {
            T t = null;
            List<T> children = new List<T>();
            int count = VisualTreeHelper.GetChildrenCount(obj);
            //尝试从content获取
            if (count == 0 && obj is ContentControl)
            {
                var objc = obj as ContentControl;
                DoPredicate<T>(objc.Content, predicate, children);
            }
            if (count == 0 && obj is ContentPresenter)
            {
                var objc = obj as ContentPresenter;
                DoPredicate<T>(objc.Content, predicate, children);
            }

            DependencyObject child = null;
            for (int i = 0; i < count; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                DoPredicate(child, predicate, children);
            }
            return children;
        }
        private static void DoPredicate<T>(object obj, Func<DependencyObject, bool> predicate, List<T> list) where T : FrameworkElement
        {
            if (obj == null) return;
            var cc = obj as DependencyObject;
            if (cc == null) return;
            if (cc is T && predicate(cc))
                list.Add(cc as T);
            list.AddRange(cc.FindChildren<T>(predicate));
        }
        #endregion

        /// <summary>
        /// 判断依赖对象是否包含在 <see cref="Popup"/> 中。
        /// </summary>
        /// <param name="visual">要判断的依赖对象。</param>
        /// <returns></returns>
        public static bool IsPopupChild(this DependencyObject visual)
        {
            return IsPopupChild(visual, out Popup popup);
        }

        /// <summary>
        /// 获取特定依赖对象所在的 <see cref="Popup"/>。
        /// </summary>
        /// <param name="visual">依赖对象。</param>
        /// <param name="popup">依赖对象所在的Popup。</param>
        /// <returns></returns>
        public static bool IsPopupChild(this DependencyObject visual, out Popup popup)
        {
            popup = null;
            var presentationSource = PresentationSource.FromDependencyObject(visual);
            if (presentationSource == null)
                return false;

            var rootVisual = presentationSource.RootVisual;
            if (rootVisual is FrameworkElement fe && fe.Parent is Popup parentPopup)
            {
                popup = parentPopup;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取特定依赖对象所在的 <see cref="Popup"/>。
        /// </summary>
        /// <param name="visual">依赖对象。</param>
        /// <returns>若依赖对象在Popup中，则返回依赖对象所在的Popup，否则返回Null。</returns>
        public static Popup FindPopup(this DependencyObject visual)
        {
            IsPopupChild(visual, out Popup popup);
            return popup;
        }

        /// <summary>
        /// Disconnect a visual from it's parent.
        /// </summary>
        /// <param name="parent">parent visual in the visual tree.</param>
        /// <param name="visual">visual which need to disconnect.</param>
        /// <remarks>
        /// Be aware,disconnect a visual from it's parent also break any exist binding.
        /// </remarks>
        public static void Disconnect(this DependencyObject visual, DependencyObject parent)
        {
            DisconnectImpl(visual, parent, out DependencyObject realParent);
        }

        /// <summary>
        /// Disconnect a visual from it's parent.
        /// </summary>
        /// <param name="parent">parent visual in the visual tree.</param>
        /// <param name="visual">visual which need to disconnect.</param>
        /// <param name="disconnectMethod">自定义的视图树移除方法，参数1为要移除的视图，参数2为要移除视图的父视图。</param>
        /// <remarks>
        /// Be aware,disconnect a visual from it's parent also break any exist binding.
        /// </remarks>
        public static void Disconnect(this DependencyObject visual, DependencyObject parent, Action<DependencyObject, DependencyObject> disconnectMethod)
        {
            if (!DisconnectImpl(visual, parent, out DependencyObject realParent))
            {
                disconnectMethod?.Invoke(visual, realParent);
            }
        }

        private static bool DisconnectImpl(DependencyObject visual, DependencyObject parent, out DependencyObject realParent)
        {
            if (visual == null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            if (parent == null)
            {
                parent = VisualTreeHelper.GetParent(visual);
            }

            realParent = parent;

            if (parent == null)
                return true;

            if (parent is Popup popup)
            {
                popup.Child = null;
                return true;
            }
            if (parent is Panel panel)
            {
                panel.Children.Remove(visual as UIElement);
                return true;
            }

            if (parent is Decorator decorator)
            {
                if (decorator.Child == visual)
                {
                    decorator.Child = null;
                }
                return true;
            }

            if (parent is ContentPresenter contentPresenter)
            {
                if (contentPresenter.Content == visual)
                {
                    contentPresenter.Content = null;
                }
                return true;
            }

            if (parent is ContentControl contentControl)
            {
                if (contentControl.Content == visual)
                {
                    contentControl.Content = null;
                }
                return true;
            }

            if (parent is Viewport2DVisual3D viewport)
            {
                if (viewport.Visual == visual)
                {
                    viewport.Visual = null;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Disconnect a visual from it's parent then connect to a new parent.
        /// </summary>
        /// <param name="source">The old parent visual in the visual tree.</param>
        /// <param name="target">The new parent visual in the visual tree.</param>
        /// <param name="visual">visual which need to disconnect.</param>
        public static void Connect(this DependencyObject visual, DependencyObject source, DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (visual == target)
            {
                return;
            }

            visual.Disconnect(source);

            DependencyObject child = visual;
            if (target is Panel panel)
            {
                var element = child as UIElement;
                if (!panel.Children.Contains(element) && element != null)
                {
                    panel.Children.Add(element);
                }
                else
                {
                    throw new InvalidOperationException($"The {visual} is already a child of {target}!");
                }
                return;
            }

            if (target is Decorator decorator)
            {
                if (decorator.Child != child && decorator.Child != null)
                {
                    throw new InvalidOperationException($"{target} has another child!");
                }
                decorator.Child = child as UIElement;
                return;
            }

            if (target is ContentPresenter contentPresenter)
            {
                if (contentPresenter.Content != child && contentPresenter.Content != null)
                {
                    throw new InvalidOperationException($"{target} contains other content!");
                }
                contentPresenter.Content = child;
                return;
            }

            if (target is ContentControl contentControl)
            {
                if (contentControl.Content != child && contentControl.Content != null)
                {
                    throw new InvalidOperationException($"{target} contains other content!");
                }
                contentControl.Content = child;
                return;
            }
        }
    }
}
