using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tryit.Wpf
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ComboBox" />
    public class EnumComboBox : ComboBox
    {
        /// <summary>
        /// 所有类型的缓存
        /// </summary>
        private static readonly IDictionary<Type, EnumDisplay[]> EnumInfoMaps = new ConcurrentDictionary<Type, EnumDisplay[]>();

        /// <summary>
        /// 当前列表的绑定项
        /// </summary>
        private readonly ObservableCollection<EnumDisplay> enumInfos = new();

        /// <summary>
        /// Initializes the <see cref="EnumComboBox"/> class.
        /// </summary>
        static EnumComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumComboBox), new FrameworkPropertyMetadata(typeof(ComboBox)));
        }

        [DebuggerDisplay("{DisplayName,nq} : {Value}")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class EnumDisplay
        {
            public readonly object? Value;
            public readonly int HashCode;

            public EnumDisplay(object? value, int hashCode, string? displayName)
            {
                Value = value;
                HashCode = hashCode;
                DisplayName = displayName;
            }

            public string? DisplayName { get; private set; }

            public override string ToString()
            {
                return DisplayName ?? string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumComboBox"/> class.
        /// </summary>
        public EnumComboBox()
        {
            ItemsSource = enumInfos;
            DisplayMemberPath = nameof(EnumDisplay.DisplayName);
        }

        /// <summary>
        /// The enum type property
        /// </summary>
        public static DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType),
            typeof(Type),
            typeof(EnumComboBox),
            new FrameworkPropertyMetadata(
                null,
                (s, e) =>
                {
                    if (s is not EnumComboBox @enum)
                    {
                        return;
                    }

                    @enum.enumInfos.Clear();

                    if (e.NewValue is not Type type || type.IsEnum == false)
                    {
                        return;
                    }

                    if (EnumInfoMaps.TryGetValue(type, out var infos) == false)
                    {
                        EnumInfoMaps[type] = infos = type.GetFields()
                            .Where(i => i.IsStatic)
                            .Select(i => new
                            {
                                Field = i,
                                Value = i.GetValue(null)!,
                                i.Name,
                                DisplayName = i.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? i.Name,
                            })
                            .Select(i => new EnumDisplay(i.Value, i.Value.GetHashCode(), i.DisplayName))
                            .ToArray();
                    }

                    for (int i = 0; i < infos.Length; i++)
                    {
                        @enum.enumInfos.Add(infos[i]);
                    }

                    @enum.TryRemoveIgnores(@enum.IgnoreValues);
                    @enum.TrySetCurrent(@enum.EnumValue);
                }
            )
        );

        /// <summary>
        /// 当前枚举类型
        /// </summary>
        public Type EnumType
        {
            get => (Type)GetValue(EnumTypeProperty);
            set => SetValue(EnumTypeProperty, value);
        }

        #region 绑定列表操作


        private void TryRemoveIgnores(IEnumerable? ignores)
        {
            if (ignores is not null)
            {
                foreach (object? ignore in ignores)
                {
                    if (ignore is not null)
                    {
                        int hashCode = ignore.GetHashCode();

                        if (enumInfos.FirstOrDefault(i => i.HashCode == hashCode) is EnumDisplay info)
                        {
                            enumInfos.Remove(info);
                        }
                    }
                }
            }
        }

        private void TrySetCurrent(object enumValue)
        {
            if (enumValue is null)
            {
                SelectedIndex = -1;
                return;
            }

            int hashCode = enumValue.GetHashCode();

            for (int i = 0; i < enumInfos.Count; i++)
            {
                if (enumInfos[i].HashCode == hashCode)
                {
                    SelectedIndex = i;
                }
            }
        }

        #endregion

        /// <summary>
        /// 当前选中的枚举值
        /// </summary>
        public static DependencyProperty EnumValueProperty = DependencyProperty.Register(
            nameof(EnumValue),
            typeof(object),
            typeof(EnumComboBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) =>
                {
                    if (s is EnumComboBox @enum)
                    {
                        @enum.TrySetCurrent(e.NewValue);
                    }
                }
            )
        );

        /// <summary>
        /// 当前选中的枚举值
        /// </summary>
        public object EnumValue
        {
            get => GetValue(EnumValueProperty);
            set => SetValue(EnumValueProperty, value);
        }

        /// <summary>
        /// The empty value property
        /// </summary>
        public static DependencyProperty EmptyValueProperty = DependencyProperty.Register(
            nameof(EmptyValue),
            typeof(string),
            typeof(EnumComboBox),
            new FrameworkPropertyMetadata(
                "None",
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) =>
                {
                    if (s is EnumComboBox @enum)
                    {
                        @enum.TrySetCurrent(@enum.EnumValue);
                    }
                }
            )
        );

        /// <summary>
        /// 空值
        /// </summary>
        public string EmptyValue
        {
            get => (string)GetValue(EmptyValueProperty);
            set => SetValue(EmptyValueProperty, value);
        }

        /// <summary>
        /// The ignore values property
        /// </summary>
        public static DependencyProperty IgnoreValuesProperty = DependencyProperty.Register(
            nameof(IgnoreValues),
            typeof(IEnumerable),
            typeof(EnumComboBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) =>
                {
                    if (s is EnumComboBox @enum)
                    {
                        @enum.TryRemoveIgnores(e.NewValue as IEnumerable);
                        @enum.TrySetCurrent(@enum.EnumValue);
                    }
                }
            )
        );

        /// <summary>
        /// 枚举忽略项
        /// </summary>
        public IEnumerable IgnoreValues
        {
            get => (IEnumerable)GetValue(IgnoreValuesProperty);
            set => SetValue(IgnoreValuesProperty, value);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            object? updateValue = null;

            if (SelectedIndex >= 0 && SelectedIndex < enumInfos.Count)
            {
                updateValue = enumInfos[SelectedIndex].Value;
            }

            SetCurrentValue(EnumValueProperty, updateValue);

            SelectionChangedEventHandler? handler = SelectionEnumValueChanged;
            handler?.Invoke(this, updateValue is null ? null! : (Enum)updateValue);

            base.OnSelectionChanged(e);
        }

        public event SelectionChangedEventHandler? SelectionEnumValueChanged;
    }

    public delegate void SelectionChangedEventHandler(object? sender, Enum enumValue);
}
