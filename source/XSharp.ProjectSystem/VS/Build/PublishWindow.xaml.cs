﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace XSharp.ProjectSystem.VS.Build
{
    /// <summary>
    /// Interaction logic for PublishWindow.xaml
    /// </summary>
    public partial class PublishWindow : DialogWindow
    {
        private PublishWindowViewModel mViewModel;

        private ManagementEventWatcher mDeviceInsertedWatcher;
        private ManagementEventWatcher mDeviceRemovedWatcher;

        public PublishWindow()
        {
            InitializeComponent();
            Initialize();
        }

        public PublishWindow(string aHelpTopic) : base(aHelpTopic)
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            mViewModel = new PublishWindowViewModel();
            DataContext = mViewModel;
        }

        public new PublishSettings ShowModal()
        {
            return base.ShowModal().GetValueOrDefault(false) ? mViewModel.ToPublishSettings() : null;
        }

        private void ReturnPublishSettings(object aSender, RoutedEventArgs aEventArgs)
        {
            if (mViewModel.PublishType == PublishType.USB && mViewModel.FormatUsbDrive)
            {
                MessageBox.Show($"The selected USB drive ({mViewModel.PublishPath}) will be formatted and its contents will be destroyed!{Environment.NewLine}Do you want to continue?",
                    "Publish", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel(object aSender, RoutedEventArgs aEventArgs)
        {
            Close();
        }

        private void BrowsePublishPath(object aSender, RoutedEventArgs aEventArgs)
        {
            var xFolderBrowserDialog = new FolderBrowserDialog();

            xFolderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
            xFolderBrowserDialog.ShowDialog();

            mViewModel.PublishPath = xFolderBrowserDialog.SelectedPath;
        }
    }

    public class PublishWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public PublishWindowViewModel()
        {
            WqlEventQuery xDeviceInsertedQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            var xDeviceInsertedWatcher = new ManagementEventWatcher(xDeviceInsertedQuery);
            xDeviceInsertedWatcher.EventArrived += DrivesChanged;
            xDeviceInsertedWatcher.Start();

            WqlEventQuery xDeviceRemovedQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            var xDeviceRemovedWatcher = new ManagementEventWatcher(xDeviceRemovedQuery);
            xDeviceRemovedWatcher.EventArrived += DrivesChanged;
            xDeviceRemovedWatcher.Start();
        }

        private void DrivesChanged(object aSender, EventArrivedEventArgs aEventArgs)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Drives)));
        }

        private void SetProperty<T>(ref T aPropertyRef, T aNewValue, string aPropertyName)
        {
            if (EqualityComparer<T>.Default.Equals(aPropertyRef, aNewValue))
            {
                aPropertyRef = aNewValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
            }
        }

        public IEnumerable<string> Drives
        {
            get
            {
                return DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).Select(d => d.RootDirectory.FullName);
            }
        }

        private PublishType mPublishType;
        public PublishType PublishType
        {
            get
            {
                return mPublishType;
            }
            set
            {
                SetProperty(ref mPublishType, value, nameof(PublishType));
            }
        }

        private string mPublishPath;
        public string PublishPath
        {
            get
            {
                return mPublishPath;
            }
            set
            {
                SetProperty(ref mPublishPath, value, nameof(PublishPath));
            }
        }

        private bool mFormatUsbDrive = true;
        public bool FormatUsbDrive
        {
            get
            {
                return mFormatUsbDrive;
            }
            set
            {
                SetProperty(ref mFormatUsbDrive, value, nameof(FormatUsbDrive));
            }
        }
        
        public PublishSettings ToPublishSettings()
        {
            return new PublishSettings(PublishType);
        }
    }
}
