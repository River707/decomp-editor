using DecompEditor.Utils;
using DecompEditor.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DecompEditor.Editors {
  /// <summary>
  /// Interaction logic for OverworldEditorView.xaml
  /// </summary>
  public partial class OverworldEditorView : UserControl {
    public OverworldEditorView() {
      InitializeComponent();
      ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      if (e.PropertyName == string.Empty) {
        /// Apply sorting and filtering to the item lists if all properties changed.
        var identifierSortDesc = new SortDescription("Identifier", ListSortDirection.Ascending);
        animList.Items.SortDescriptions.Add(identifierSortDesc);
        objectList.Items.SortDescriptions.Add(identifierSortDesc);
        objectList.Items.Filter = obj => (obj as EventObject).Frames.Count != 0;
        objectList.Items.IsLiveSorting = true;
        var palListView = (ListCollectionView)CollectionViewSource.GetDefaultView(paletteList.ItemsSource);
        palListView.CustomSort = Comparer<EventObjectPalette>.Create((lhs, rhs) => lhs.Identifier.CompareToNatural(rhs.Identifier));
        spriteList.Items.SortDescriptions.Add(identifierSortDesc);
        spriteList.Items.IsLiveSorting = true;
        removeIndexButtom.IsEnabled = ViewModel.AnimTableIndex != 0;
        return;
      }
      if (e.PropertyName == "CurrentFrame") {
        handleFrameChange();
        return;
      }
      if (e.PropertyName == "AnimTableIndex") {
        removeIndexButtom.IsEnabled = ViewModel.AnimTableIndex != 0;
        return;
      }
    }

    public OverworldEditorViewModel ViewModel => DataContext as OverworldEditorViewModel;

    private void objectList_SelectionChanged(object sender, SelectionChangedEventArgs evt) => ViewModel.CurrentObject = objectList.SelectedItem as EventObject;

    private void EditSpriteButton_Click(object sender, RoutedEventArgs e) {
      var window = new OverworldPicEditorWindow(spriteList.SelectedItem as EventObjectPic) {
        Owner = Application.Current.MainWindow,
        WindowStartupLocation = WindowStartupLocation.CenterOwner
      };
      window.ShowDialog();
      handleFrameChange();
    }

    private void copyPreviousFrames() {
      int curIndex = ViewModel.AnimTableIndex;
      ObservableCollection<EventObject.Frame> frames = ViewModel.CurrentObject.Frames;
      int neededFrames = (curIndex - frames.Count) + 1;
      if (neededFrames <= 0)
        return;

      EventObject.Frame lastValidFrame = frames[Math.Min(curIndex - 1, frames.Count - 1)];
      int maxFrame = FileUtils.getImageWidth(lastValidFrame.Pic.FullPath);
      maxFrame /= ViewModel.CurrentObject.Width;
      for (int i = 0, e = neededFrames; i < e; ++i) {
        frames.Add(new EventObject.Frame() {
          Pic = lastValidFrame.Pic,
          Index = Math.Min(maxFrame - 1, lastValidFrame.Index + i + 1)
        });
      }
    }

    private void removeIndexButtom_Click(object sender, RoutedEventArgs evt) {
      ObservableCollection<EventObject.Frame> frames = ViewModel.CurrentObject.Frames;
      for (int i = ViewModel.AnimTableIndex, e = frames.Count; i != e; ++i)
        frames.RemoveAt(ViewModel.AnimTableIndex);
      ViewModel.AnimTableIndex -= 1;
    }

    private void handleFrameChange() {
      if (!IsInitialized || ViewModel.CurrentObject == null) {
        overworldPic.Source = null;
        return;
      }
      EventObject.Frame currentFrame = ViewModel.CurrentFrame;

      // If the frame is invalid, copy the previous frames and raise an event.
      if (currentFrame == null) {
        copyPreviousFrames();
        ViewModel.RaisePropertyChanged("AnimTableIndex");
        ViewModel.RaisePropertyChanged("CurrentFrame");
        return;
      }
      indexCount.Content = "of " + (ViewModel.CurrentObject.Frames.Count - 1);

      BitmapImage fileBitmap = FileUtils.loadBitmapImage(currentFrame.Pic.FullPath);

      // Make sure the object width/height is appropriate for the actual image.
      EventObject currentObject = ViewModel.CurrentObject;
      currentObject.Width = Math.Min(currentObject.Width, fileBitmap.PixelWidth);
      currentObject.Height = Math.Min(currentObject.Height, fileBitmap.PixelHeight);

      // Set max for framecount.
      spriteFrame.Maximum = ((fileBitmap.PixelWidth / 8) / (spriteWidth.Value / 8)) - 1;
      currentFrame.Index = Math.Min(currentFrame.Index, (int)spriteFrame.Maximum);

      // Check to see if the width/height of the object is the same as the image.
      if (currentObject.Width == fileBitmap.PixelWidth &&
          currentObject.Height == fileBitmap.PixelHeight) {
        overworldPic.Source = fileBitmap;
        spriteFrame.IsEnabled = false;
        return;
      }

      // Otherwise, slice the image to get the specific frame.
      overworldPic.Source = new CroppedBitmap(fileBitmap, new Int32Rect(
        currentObject.Width * currentFrame.Index, /*y=*/0, currentObject.Width,
        currentObject.Height));
      spriteFrame.IsEnabled = true;
    }

    private void spriteList_SelectionChanged(object sender, SelectionChangedEventArgs e) => handleFrameChange();

    private void spriteFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) => handleFrameChange();
  }
}
