using Parse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ParseTodo
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : ParseTodo.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            DefaultViewModel["SelectedTodo"] = null;
            DefaultViewModel["IsEditing"] = false;
            DefaultViewModel["CanEdit"] = false;
            DefaultViewModel["IsSaving"] = false;
            Refresh();
        }

        private async void Refresh()
        {
            var query = from item in ParseObject.GetQuery(TodoItem.ClassName)
                        orderby item.CreatedAt
                        select item;

            try
            {
                DefaultViewModel["IsLoading"] = true;

                var allItems = from item in await query.FindAsync()
                               select new TodoItem(item);
                var itemList = new ObservableCollection<TodoItem>(allItems);
                DefaultViewModel["SelectedTodo"] = null;
                DefaultViewModel["TodoItems"] = itemList;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
            finally
            {
                DefaultViewModel["IsLoading"] = false;
            }
        }

        private async void HandleError(Exception e)
        {
            var dialog = new MessageDialog(e.Message, "An error occurred");
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void AddTodoItem(object sender, RoutedEventArgs e)
        {
            var newItem = new TodoItem();
            ((ObservableCollection<TodoItem>)DefaultViewModel["TodoItems"]).Add(newItem);
            DefaultViewModel["SelectedTodo"] = newItem;
            ToggleEditing(this, e);
        }

        private void RefreshTodoItems(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void SaveCurrentItem(object sender, RoutedEventArgs e)
        {
            DefaultViewModel["IsEditing"] = false;
            DefaultViewModel["CanEdit"] = true;
            DefaultViewModel["IsSaving"] = true;
            await ((TodoItem)DefaultViewModel["SelectedTodo"]).SaveAsync();
            DefaultViewModel["IsSaving"] = false;
        }

        private async void DeleteCurrentItem(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Are you sure you want to delete this Todo item?", "Delete");
            var yesCommand = new UICommand("Yes");
            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(new UICommand("No"));
            dialog.CancelCommandIndex = 1;
            dialog.DefaultCommandIndex = 0;

            var result = await dialog.ShowAsync();
            if (result == yesCommand)
            {
                var item = (TodoItem)DefaultViewModel["SelectedTodo"];
                ((ObservableCollection<TodoItem>)DefaultViewModel["TodoItems"]).Remove(item);
                DefaultViewModel["SelectedTodo"] = null;
                DefaultViewModel["IsEditing"] = false;
                DefaultViewModel["IsSaving"] = true;
                await item.BackingObject.DeleteAsync();
                DefaultViewModel["IsSaving"] = false;
            }
        }

        private void RevertCurrentItem(object sender, RoutedEventArgs e)
        {
            ((TodoItem)DefaultViewModel["SelectedTodo"]).Revert();
            DefaultViewModel["IsEditing"] = false;
            DefaultViewModel["CanEdit"] = true;
        }

        private void ToggleEditing(object sender, RoutedEventArgs e)
        {
            bool newIsEditing = !(bool)DefaultViewModel["IsEditing"];
            DefaultViewModel["IsEditing"] = newIsEditing;
            DefaultViewModel["CanEdit"] = !newIsEditing;
        }

        private void TodoSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DefaultViewModel["CanEdit"] = ((Selector)sender).SelectedItem != null;
        }

        private void MarkCompleted(object sender, RoutedEventArgs e)
        {
            DefaultViewModel["IsSaving"] = true;
            ((TodoItem)((Control)sender).DataContext).SaveAsync();
            DefaultViewModel["IsSaving"] = false;
        }
    }
}
