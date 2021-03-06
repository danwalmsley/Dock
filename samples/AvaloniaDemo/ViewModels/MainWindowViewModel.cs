﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model;

namespace AvaloniaDemo.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private IDockFactory _factory;
        private IDock _layout;

        public IDockFactory Factory
        {
            get => _factory;
            set => Update(ref _factory, value);
        }

        public IDock Layout
        {
            get => _layout;
            set => Update(ref _layout, value);
        }
    }
}
