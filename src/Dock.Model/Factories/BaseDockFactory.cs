// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dock.Model.Factories
{
    /// <summary>
    /// Dock factory base.
    /// </summary>
    public abstract class BaseDockFactory : IDockFactory
    {
        /// <inheritdoc/>
        public virtual IDictionary<string, Func<object>> ContextLocator { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, Func<IDockHost>> HostLocator { get; set; }

        /// <inheritdoc/>
        public virtual object GetContext(string id, object context)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<object> locator = null;
                if (ContextLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            return context;
        }

        /// <inheritdoc/>
        public virtual IDockHost GetHost(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Func<IDockHost> locator = null;
                if (HostLocator?.TryGetValue(id, out locator) == true)
                {
                    return locator?.Invoke();
                }
            }
            throw new Exception($"Dock host with {id} is not registered.");
        }

        /// <inheritdoc/>
        public virtual void Update(IDockWindow window, object context)
        {
            window.Host = GetHost(window.Id);
            window.Context = GetContext(window.Id, context);

            if (window.Layout != null)
            {
                Update(window.Layout, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDockWindow> windows, object context)
        {
            foreach (var window in windows)
            {
                Update(window, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IDock view, object context)
        {
            view.Context = GetContext(view.Id, context);
            view.Factory = this;

            if (view.Windows != null)
            {
                Update(view.Windows, context);
            }

            if (view.Views != null)
            {
                Update(view.Views, context);
            }
        }

        /// <inheritdoc/>
        public virtual void Update(IList<IDock> views, object context)
        {
            foreach (var view in views)
            {
                Update(view, context);
            }
        }

        /// <inheritdoc/>
        public abstract IDock CreateLayout();

        /// <inheritdoc/>
        public abstract void InitLayout(IDock layout, object context);
    }
}
