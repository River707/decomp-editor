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
    protected bool SetAndTrackItemUpdates<T, V>(ref T field, T newValue,
                                                ObservableObject parent,
                                                [CallerMemberName] string propertyName = null) where T : System.Collections.ObjectModel.ObservableCollection<V> {
      var handler = new PropertyChangedEventHandler((sender, e) => {
        parent.RaisePropertyChanged(propertyName);
      });
      newValue.CollectionChanged += (sender, e) => {
        parent.RaisePropertyChanged(propertyName);
        if (e.Action == NotifyCollectionChangedAction.Reset) {
          foreach (object item in sender as System.Collections.ObjectModel.ObservableCollection<V>)
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
      foreach (object item in newValue)
        if (item is INotifyPropertyChanged)
          ((INotifyPropertyChanged)item).PropertyChanged += handler;

      return Set(ref field, newValue, propertyName);
    }
    protected bool SetAndTrackItemUpdates<T>(ref ObservableCollection<T> field, ObservableCollection<T> newValue,
                                            ObservableObject parent,
                                            [CallerMemberName] string propertyName = null) {
      return SetAndTrackItemUpdates<ObservableCollection<T>, T>(ref field, newValue, parent, propertyName);
    }
    protected bool SetAndTrackItemUpdates<T>(ref SortedObservableCollection<T> field, SortedObservableCollection<T> newValue,
                                            ObservableObject parent,
                                            [CallerMemberName] string propertyName = null) {
      return SetAndTrackItemUpdates<SortedObservableCollection<T>, T>(ref field, newValue, parent, propertyName);
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

  public class SortedObservableCollection<T> : ObservableCollection<T> {
    public SortedObservableCollection() { }
    public SortedObservableCollection(IEnumerable<T> items) {
      foreach (var item in items)
        Add(item);
    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      T item = (T)sender;
      Comparer<T> comparer = Comparer<T>.Default;
      int itemIndex = IndexOf(item);
      if ((itemIndex != 0 && comparer.Compare(item, this[itemIndex - 1]) == -1) ||
          (itemIndex != Count - 1 && comparer.Compare(item, this[itemIndex + 1]) == 1)) {
        RemoveAt(itemIndex);
        Add(item);
      }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if (e.Action == NotifyCollectionChangedAction.Reset) {
        foreach (object item in Items)
          if (item is INotifyPropertyChanged)
            ((INotifyPropertyChanged)item).PropertyChanged += Item_PropertyChanged;
        base.OnCollectionChanged(e);
        return;
      }

      if (e.OldItems != null) {
        foreach (object item in e.OldItems) {
          if (item is INotifyPropertyChanged)
            ((INotifyPropertyChanged)item).PropertyChanged -= Item_PropertyChanged;
        }
      }
      if (e.NewItems != null) {
        foreach (object item in e.NewItems) {
          if (item is INotifyPropertyChanged)
            ((INotifyPropertyChanged)item).PropertyChanged += Item_PropertyChanged;
        }
      }

      base.OnCollectionChanged(e);
    }

    public new void Insert(int _, T item) => Add(item);

    public new void Add(T item) {
      ObservableCollection<T> baseCollection = this; 
      if (Items.Count == 0) {
        baseCollection.Add(item);
        return;
      }

      Comparer<T> comparer = Comparer<T>.Default;
      for (int i = 0; i < Items.Count; i++) {
        if (comparer.Compare(Items[i], item) == 1) {
          baseCollection.Insert(i, item);
          return;
        }
      }
      baseCollection.Add(item);
    }
  }
}
