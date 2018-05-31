﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Runtime.Serialization;
using ReactiveUI;

namespace Dock.Model
{
    /// <summary>
    /// Dock base class.
    /// </summary>
    public abstract class DockBase : ViewBase, IDock
    {
        private NavigateAdapter _navigate;
        private IList<IView> _views;
        private IView _currentView;
        private IView _defaultView;
        private IList<IDockWindow> _windows;
        private string _dock;
        private IDockFactory _factory;

        /// <summary>
        /// Initializes new instance of the <see cref="DockBase"/> class.
        /// </summary>
        public DockBase()
        {
            _navigate = new NavigateAdapter(this);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public IList<IView> Views
        {
            get => _views;
            set => this.RaiseAndSetIfChanged(ref _views, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IView CurrentView
        {
            get => _currentView;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentView, value);
                this.RaisePropertyChanged(nameof(CanGoBack));
                this.RaisePropertyChanged(nameof(CanGoForward));
            }
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IView DefaultView
        {
            get => _defaultView;
            set => this.RaiseAndSetIfChanged(ref _defaultView, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoBack => _navigate.CanGoBack;

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool CanGoForward => _navigate.CanGoForward;

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<IDockWindow> Windows
        {
            get => _windows;
            set => this.RaiseAndSetIfChanged(ref _windows, value);
        }

        /// <inheritdoc/>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Dock
        {
            get => _dock;
            set => this.RaiseAndSetIfChanged(ref _dock, value);
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        /// <inheritdoc/>
        public virtual void GoBack()
        {
            _navigate.GoBack();
        }

        /// <inheritdoc/>
        public virtual void GoForward()
        {
            _navigate.GoForward();
        }

        /// <inheritdoc/>
        public virtual void Navigate(object root)
        {
            _navigate.Navigate(root, true);
        }

        /// <inheritdoc/>
        public virtual void ShowWindows()
        {
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Present(false);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void HideWindows()
        {
            if (Windows != null)
            {
                foreach (var window in Windows)
                {
                    window.Destroy();
                }
            }
        }
    }
}