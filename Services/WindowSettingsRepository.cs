﻿using System;
using System.Windows;
using Jamiras.Components;

namespace Jamiras.Services
{
    [Export(typeof(IWindowSettingsRepository))]
    internal class WindowSettingsRepository : IWindowSettingsRepository
    {
        [ImportingConstructor]
        public WindowSettingsRepository(IPersistantDataRepository persistantDataRepository)
        {
            _persistantDataRepository = persistantDataRepository;
        }

        private readonly IPersistantDataRepository _persistantDataRepository;

        private static string GetPrefix(Window window)
        {
            return window.Name ?? window.GetType().Name;
        }

        #region IWindowSettingsRepository Members

        public void RememberSettings(Window window)
        {
            string prefix = GetPrefix(window);

            _persistantDataRepository.BeginUpdate();

            string location = String.Format("{0},{1}", window.Left, window.Top);
            _persistantDataRepository.SetValue(prefix + ".WindowLocation", location);

            string size = String.Format("{0}x{1}", window.Width, window.Height);
            _persistantDataRepository.SetValue(prefix + ".WindowSize", size);

            _persistantDataRepository.EndUpdate();
        }

        public void RestoreSettings(Window window)
        {
            string prefix = GetPrefix(window);

            string location = _persistantDataRepository.GetValue(prefix + ".WindowLocation");
            if (!String.IsNullOrEmpty(location))
            {
                string[] parts = location.Split(',');
                if (parts.Length == 2)
                {
                    int value;
                    if (Int32.TryParse(parts[0], out value))
                        window.Left = value;
                    if (Int32.TryParse(parts[1], out value))
                        window.Top = value;
                }
            }

            string size = _persistantDataRepository.GetValue(prefix + ".WindowSize");
            if (!String.IsNullOrEmpty(size))
            {
                string[] parts = size.Split('x');
                if (parts.Length == 2)
                {
                    int value;
                    if (Int32.TryParse(parts[0], out value))
                        window.Width = value;
                    if (Int32.TryParse(parts[1], out value))
                        window.Height = value;
                }
            }
        }

        #endregion
    }
}
