using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace DecompEditor.Utils {
  public class ObservableObject : GalaSoft.MvvmLight.ObservableObject {
    Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> childHandlers;

    public override void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
      if (!Project.Instance?.IsLoading ?? true)
        base.RaisePropertyChanged(propertyName);
    }
    public override void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression) {
      if (!Project.Instance?.IsLoading ?? true)
        base.RaisePropertyChanged(propertyExpression);
    }

    private void addChildHandler(INotifyPropertyChanged child, string propertyName) {
      if (child == null)
        return;
      if (childHandlers == null)
        childHandlers = new Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler>();
      PropertyChangedEventHandler handler = (sender, e) => RaisePropertyChanged(propertyName);
      childHandlers.Add(child, handler);
      child.PropertyChanged += handler;
    }
    private void removeChildHandler(INotifyPropertyChanged child) {
      if (child == null || childHandlers == null)
        return;
      if (childHandlers.TryGetValue(child, out PropertyChangedEventHandler handler))
        child.PropertyChanged -= handler;
    }

    protected bool SetAndTrack<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
      removeChildHandler(field as INotifyPropertyChanged);
      addChildHandler(newValue as INotifyPropertyChanged, propertyName);
      return Set(propertyName, ref field, newValue);
    }
    protected bool SetAndTrackItemUpdates<ValueT>(ref ObservableCollection<ValueT> field,
                                                  ObservableCollection<ValueT> newValue,
                                                  ObservableObject parent,
                                                  [CallerMemberName] string propertyName = null) {
      var handler = new PropertyChangedEventHandler((sender, e) => {
        parent.RaisePropertyChanged(propertyName);
      });
      newValue.CollectionChanged += (sender, e) => {
        parent.RaisePropertyChanged(propertyName);
        if (e.Action == NotifyCollectionChangedAction.Reset) {
          foreach (object item in sender as ObservableCollection<ValueT>)
            if (item is INotifyPropertyChanged)
              ((INotifyPropertyChanged)item).PropertyChanged += handler;
          return;
        }

        if (e.OldItems != null) {
          foreach (object item in e.OldItems) {
            if (item is INotifyPropertyChanged)
              ((INotifyPropertyChanged)item).PropertyChanged -= handler;
          }
        }
        if (e.NewItems != null) {
          foreach (object item in e.NewItems) {
            if (item is INotifyPropertyChanged)
              ((INotifyPropertyChanged)item).PropertyChanged += handler;
          }
        }
      };
      foreach(object item in newValue)
        if (item is INotifyPropertyChanged)
          ((INotifyPropertyChanged)item).PropertyChanged += handler;

      return Set(ref field, newValue, propertyName);
    }
  }
  public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T> {
    Project.LoadEventHandler handler;

    public ObservableCollection() { }
    public ObservableCollection(IEnumerable<T> items) : base(items) { }
    ~ObservableCollection() => Project.Instance.Loaded -= handler;

    void RaiseCollectionChanged() 
      => base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if (!Project.Instance?.IsLoading ?? true)
        base.OnCollectionChanged(e);
      else if (handler == null) {
        handler = new Project.LoadEventHandler(() => RaiseCollectionChanged());
        Project.Instance.Loaded += handler;
      }
    }
  }
}
