﻿using BusinessLogicModule.Interfaces;
using BusinessLogicModule.Services;
using NLog;
using SharedServicesModule;
using SharedServicesModule.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using UIModule.Utils;
using Windows.UI.Xaml;

namespace UIModule.ViewModels
{
    public class ChangeTaskViewModel : NavigateViewModel
    {
        private static Logger logger;
        IUserRepository _userRepository;
        ITaskRepository _taskRepository;
        IProjectRepository _projectRepository;

        public ChangeTaskViewModel(IUserRepository UserRepository, ITaskRepository TaskRepository, IProjectRepository ProjectRepository)
        {
            logger = LogManager.GetCurrentClassLogger();
            _userRepository = UserRepository;
            _taskRepository = TaskRepository;
            _projectRepository = ProjectRepository;
        }

        #region Properties

        private string _taskName;
        public string TaskName
        {
            get { return _taskName; }
            set
            {
                _taskName = value;
                OnPropertyChanged();
            }
        }

        private string _taskDescription;
        public string TaskDescription
        {
            get { return _taskDescription; }
            set
            {
                _taskDescription = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset _taskFinishDate;
        public DateTimeOffset TaskFinishDate
        {
            get { return _taskFinishDate; }
            set
            {
                _taskFinishDate = value;
                OnPropertyChanged();
            }
        }

        private List<User> _users;
        public List<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        private Visibility _pageVisibility = Visibility.Collapsed;
        public Visibility PageVisibility
        {
            get { return _pageVisibility; }
            set
            {
                _pageVisibility = value;
                OnPropertyChanged();
            }
        }
        
        private Visibility _loadingVisibility = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get { return _loadingVisibility; }
            set
            {
                _loadingVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public ICommand Loaded
        {
            get
            {
                return new DelegateCommand(async (obj) =>
                {
                    try
                    {
                        PageVisibility = Visibility.Collapsed;
                        LoadingVisibility = Visibility.Visible;

                        Users = await _userRepository.GetUsersFromProject(Consts.ProjectId);

                        Task task = (await _taskRepository.GetTask(Consts.TaskId));
                        TaskName = task.Name;
                        TaskDescription = task.Description;
                        TaskFinishDate = task.EndDate;

                        SelectedUser = (await _userRepository.GetUser(Consts.UserName));

                        LoadingVisibility = Visibility.Collapsed;
                        PageVisibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        ErrorHandler.Show(Application.Current.Resources["m_error_download"].ToString() + "\n" + ex.Message);
                    }
                });
            }
        }

        public ICommand Back
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    try
                    {
                        NavigationService.Instance.NavigateTo(typeof(Pages.Task));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        ErrorHandler.Show(Application.Current.Resources["m_error_download"].ToString() + "\n" + ex.Message);
                    }
                });
            }
        }

        public ICommand Accept
        {
            get
            {
                return new DelegateCommand(async (obj) =>
                {
                    try
                    {
                        if (TaskName != null && TaskDescription != null && SelectedUser != null && TaskFinishDate != null && TaskFinishDate >= DateTime.Today)
                        {
                            Task task = await _taskRepository.GetTask(Consts.TaskId);
                            await _taskRepository.ChangeTask(task, TaskName, TaskDescription, SelectedUser.Id, task.StatusId, TaskFinishDate.UtcDateTime);

                            logger.Debug("user " + Consts.UserName + " added task " + TaskName + " to the project " + (await _projectRepository.GetProject(Consts.ProjectId)).Name);

                            Notification.ShowToastNotification(Application.Current.Resources["mSuccessChangeTask"].ToString());
                            NavigationService.Instance.NavigateTo(typeof(Pages.Project));
                        }
                        else
                        {
                            ErrorHandler.Show(Application.Current.Resources["m_correct_entry"].ToString());
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        ErrorHandler.Show(Application.Current.Resources["m_error_create_task"].ToString() + "\n" + ex.Message);
                    }
                });
            }
        }
        #endregion
    }
}
