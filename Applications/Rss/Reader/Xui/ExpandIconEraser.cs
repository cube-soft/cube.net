/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cube.Xui
{
    public class ExpandIconEraser
    {
        public static bool GetIsHideExpander(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHideExpanderProperty);
        }

        public static void SetIsHideExpander(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHideExpanderProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsHideExpander.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHideExpanderProperty =
            DependencyProperty.RegisterAttached("IsHideExpander", typeof(bool), typeof(ExpandIconEraser), new PropertyMetadata(false, IsHideExpander_PropertyChanged));

        private static void IsHideExpander_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem treeViewItem = d as TreeViewItem;
            if (treeViewItem == null) return;
            treeViewItem.Loaded += TreeViewItem_Loaded;
        }

        private static void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            if (treeViewItem == null) return;

            treeViewItem.Loaded -= TreeViewItem_Loaded;

            ToggleButton tg = FindChild<ToggleButton>(treeViewItem);
            if (tg != null)
            {
                tg.Visibility = Visibility.Collapsed;
            }
        }

        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            if (parent is T) return parent as T;

            T child = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var c = VisualTreeHelper.GetChild(parent, i);
                child = FindChild<T>(c);
                if (child != null) break;
            }

            return child;
        }
    }
}
