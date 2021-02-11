using DecompEditor.ProjectData;
using DecompEditor.Utils;
using DecompEditor.Views;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for AdventureEditorView.xaml
  /// </summary>
  public partial class AdventureEditorView : UserControl {
    public AdventureEditorView() {
      InitializeComponent();
    }

    public AdventureEditorViewModel ViewModel => DataContext as AdventureEditorViewModel;

    private void adventureList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ViewModel.CurrentAdventure = adventureList.SelectedItem as Adventure;
    }

    private void taskMenu_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      ViewModel.CurrentTask = taskMenu.SelectedItem as AdventureTask;
    }

    private Point? menuDragstartPoint;
    private void menu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => menuDragstartPoint = e.GetPosition(null);
    private void menu_PreviewMouseMove(object sender, MouseEventArgs e) {
      // Check that a drag is happening from the party menu.
      if (menuDragstartPoint == null /* || sender != partyMenu */)
        return;

      // Get the current mouse position
      Point mousePos = e.GetPosition(null);
      Vector diff = menuDragstartPoint.Value - mousePos;
      // test for the minimum displacement to begin the drag
      if (!(e.LeftButton == MouseButtonState.Pressed &&
          (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
          Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))) {
        return;
      }

      // Get the pokemon from the row being dragged.
      DataGridRow dataGridRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (dataGridRow == null)
        return;
      int rowIndex = dataGridRow.GetIndex();

      // Initialize the drag & drop operation.
      var dataObj = new DataObject("origIndex", rowIndex);
      dataObj.SetData("DragSource", sender);
      DragDrop.DoDragDrop(sender as DependencyObject, dataObj, DragDropEffects.Copy);
      menuDragstartPoint = null;
    }

    private void menu_PreviewMouseUp(object sender, MouseButtonEventArgs e) => menuDragstartPoint = null;

    private void menu_Drop(object sender, DragEventArgs e) {
      var dg = sender as DataGrid;
      if (dg == null)
        return;
      DataGridRow newGridRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (newGridRow == null)
        return;
      int newRowIndex = newGridRow.GetIndex();

      var dgSrc = e.Data.GetData("DragSource") as DataGrid;
      object origIndexObj = e.Data.GetData("origIndex");
      if (dgSrc == null || origIndexObj == null || (int)origIndexObj == newRowIndex)
        return;
      if (sender == taskMenu) {
        if (newRowIndex < ViewModel.CurrentAdventure.Tasks.Count)
          ViewModel.CurrentAdventure.Tasks.Move((int)origIndexObj, newRowIndex);
      } else if (sender == preconditionMenu) {
        if (newRowIndex < ViewModel.CurrentTask.PreConditions.Count)
          ViewModel.CurrentTask.PreConditions.Move((int)origIndexObj, newRowIndex);
      } else if (sender == failureconditionMenu) {
        if (newRowIndex < ViewModel.CurrentTask.FailureConditions.Count)
          ViewModel.CurrentTask.FailureConditions.Move((int)origIndexObj, newRowIndex);
      }
    }

    private void menu_PreviewDragOver(object sender, DragEventArgs e) {
      // Don't allow drag if there is only one item.
      bool hasOneItem = false;
      if (sender == taskMenu) {
        hasOneItem = ViewModel.CurrentAdventure.Tasks.Count == 1;
      } else if (sender == preconditionMenu) {
        hasOneItem = ViewModel.CurrentTask.PreConditions.Count == 1;
      } else if (sender == failureconditionMenu) {
        hasOneItem = ViewModel.CurrentTask.FailureConditions.Count == 1;
      }
      if (hasOneItem)
        e.Effects = DragDropEffects.None;
    }

    private void menu_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      DataGridRow clickedRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (clickedRow == null || !clickedRow.IsNewItem)
        return;
      if (sender == taskMenu) {
        ViewModel.CurrentAdventure.AddDefaultTask();
        taskMenu.SelectedIndex = ViewModel.CurrentAdventure.Tasks.Count - 1;
      }
    }

    private void menu_PreviewKeyDown(object sender, KeyEventArgs e) {
      DataGridRow clickedRow = ((DependencyObject)e.OriginalSource).FindVisualParent<DataGridRow>();
      if (e.Key != Key.Delete || clickedRow == null || clickedRow.IsNewItem)
        return;

      int removeIndex = clickedRow.GetIndex();
      if (sender == taskMenu) {
        if (ViewModel.CurrentAdventure.Tasks.Count == 1)
          return;
        ViewModel.CurrentAdventure.Tasks.RemoveAt(removeIndex);
        taskMenu.SelectedIndex = Math.Max(0, removeIndex - 1);
      } else if (sender == preconditionMenu) {
        ViewModel.CurrentTask.PreConditions.RemoveAt(removeIndex);
        preconditionMenu.SelectedIndex = Math.Max(0, removeIndex - 1);
      } else if (sender == failureconditionMenu) {
        ViewModel.CurrentTask.FailureConditions.RemoveAt(removeIndex);
        failureconditionMenu.SelectedIndex = Math.Max(0, removeIndex - 1);
      }
    }

    private void NewConditionBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var combo = sender as ComboBox;
      if (combo.SelectedIndex == -1)
        return;

      string item = ((ComboBoxItem)combo.SelectedValue).Content as string;
      AdventureTaskCondition condition;
      if (item == "Flag Condition") {
        condition = AdventureTaskFlagCondition.createDefault();
      } else if (item == "Task Condition") {
        condition = AdventureTaskTaskCondition.createDefault(ViewModel.CurrentTask);
      } else {
        condition = AdventureTaskVariableCondition.createDefault();
      }

      if (preconditionMenu.IsAncestorOf(combo)) {
        ViewModel.CurrentTask.PreConditions.Add(condition);
        preconditionMenu.SelectedIndex = ViewModel.CurrentTask.PreConditions.Count - 1;
        preconditionMenu.Focus();
      } else {
        ViewModel.CurrentTask.FailureConditions.Add(condition);
        failureconditionMenu.SelectedIndex = ViewModel.CurrentTask.FailureConditions.Count - 1;
        failureconditionMenu.Focus();
      }
      combo.SelectedIndex = -1;
    }

    private void addAdventureButton_Click(object sender, RoutedEventArgs e) {
      adventureList.SelectedItem = Project.Instance.Adventures.addNewAdventure();
    }

    private void preconditionMenu_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) {
      var grid = sender as DataGrid;
      if (grid.SelectedIndex == grid.Items.Count - 1) {
        var element = grid.ItemContainerGenerator.ContainerFromIndex(grid.Items.Count - 1);
        if (element == null)
          return;

        ComboBox newItemCombo = DialogUtils.FindVisualChild<ComboBox>(element);
        newItemCombo.Focus();
        newItemCombo.IsDropDownOpen = true;
        grid.SelectedIndex = -1;
      }
    }
  }
  public class CellTemplateSelector : DataTemplateSelector {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      DataTemplate template;
      if (item != null && item.ToString() == "{DataGrid.NewItemPlaceholder}")
        template = (container as FrameworkElement).TryFindResource("PlaceholderTemplate") as DataTemplate;
      else
        template = base.SelectTemplate(item, container);

      return template;
    }
  }
}
