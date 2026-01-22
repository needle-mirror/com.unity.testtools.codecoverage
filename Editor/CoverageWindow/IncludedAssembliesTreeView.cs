using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;

using UnityEditor.TestTools.CodeCoverage.Analytics;

namespace UnityEditor.TestTools.CodeCoverage
{
#if UNITY_6000_3_OR_NEWER
    class IncludedAssembliesTreeView : TreeView<int>
#else
    class IncludedAssembliesTreeView : TreeView
#endif
    {
        string m_AssembliesToInclude;
        readonly CodeCoverageWindow m_Parent;
        const float kCheckBoxWidth = 42f;

        public float Width { get; set; } = 100f;

#if UNITY_6000_3_OR_NEWER
        public IncludedAssembliesTreeView(CodeCoverageWindow parent, string assembliesToInclude)
            : base(new TreeViewState<int>())
#else
        public IncludedAssembliesTreeView(CodeCoverageWindow parent, string assembliesToInclude)
            : base(new TreeViewState())
#endif
        {
            m_AssembliesToInclude = assembliesToInclude;
            m_Parent = parent;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

#if UNITY_6000_3_OR_NEWER
        protected override bool CanMultiSelect(TreeViewItem<int> item)
#else
        protected override bool CanMultiSelect(TreeViewItem item)
#endif
        {
            return false;
        }

#if UNITY_6000_3_OR_NEWER
        protected override TreeViewItem<int> BuildRoot()
#else
        protected override TreeViewItem BuildRoot()
#endif
        {
            string[] includeAssemblyFilters = m_AssembliesToInclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] includeAssemblies = includeAssemblyFilters
                .Select(f => AssemblyFiltering.CreateFilterRegex(f))
                .ToArray();

#if UNITY_6000_3_OR_NEWER
            TreeViewItem<int> root = new TreeViewItem<int>(-1, -1);
#else
            TreeViewItem root = new TreeViewItem(-1, -1);
#endif

            bool developerMode = EditorPrefs.GetBool("DeveloperMode", false);

            if (developerMode)
            {
                System.Reflection.Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssembliesInternal();
                int assembliesLength = assemblies.Length;

                GUIContent textContent = new GUIContent();
                for (int i = 0; i < assembliesLength; ++i)
                {
                    System.Reflection.Assembly assembly = assemblies[i];
                    bool enabled = includeAssemblies.Any(f => f.IsMatch(assembly.GetName().Name.ToLowerInvariant()));
                    root.AddChild(new AssembliesTreeViewItem() { id = i + 1, displayName = assembly.GetName().Name, Enabled = enabled });

                    textContent.text = assembly.GetName().Name;
#if UNITY_6000_3_OR_NEWER
                    float itemWidth = TreeView<int>.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
#else
                    float itemWidth = TreeView.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
#endif
                    if (Width < itemWidth)
                        Width = itemWidth;

                }
            }
            else
            {
                Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssemblies();
                int assembliesLength = assemblies.Length;

                GUIContent textContent = new GUIContent();
                for (int i = 0; i < assembliesLength; ++i)
                {
                    Assembly assembly = assemblies[i];
                    bool enabled = includeAssemblies.Any(f => f.IsMatch(assembly.name.ToLowerInvariant()));
                    root.AddChild(new AssembliesTreeViewItem() { id = i + 1, displayName = assembly.name, Enabled = enabled });

                    textContent.text = assembly.name;
#if UNITY_6000_3_OR_NEWER
                    float itemWidth = TreeView<int>.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
#else
                    float itemWidth = TreeView.DefaultStyles.label.CalcSize(textContent).x + kCheckBoxWidth;
#endif
                    if (Width < itemWidth)
                        Width = itemWidth;
                }
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            AssembliesTreeViewItem item = args.item as AssembliesTreeViewItem;
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUI.ToggleLeft(args.rowRect, args.label, item.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                item.Enabled = enabled;
                ApplyChanges();
            }
        }

        public void SelectAll()
        {
            ToggleAll(true);
        }

        public void DeselectAll()
        {
            ToggleAll(false);
        }

        public void SelectAssets()
        {
            m_AssembliesToInclude = AssemblyFiltering.GetUserOnlyAssembliesString();
            SelectFromString(m_AssembliesToInclude);
        }

        public void SelectPackages()
        {
            m_AssembliesToInclude = AssemblyFiltering.GetPackagesOnlyAssembliesString();
            SelectFromString(m_AssembliesToInclude);
        }

        private void SelectFromString(string assembliesToInclude)
        {
            string[] includeAssemblyFilters = assembliesToInclude.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] includeAssemblies = includeAssemblyFilters
                .Select(f => AssemblyFiltering.CreateFilterRegex(f))
                .ToArray();

            foreach (var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;

                bool enabled = includeAssemblies.Any(f => f.IsMatch(childItem.displayName.ToLowerInvariant()));
                if (searchString == null)
                    childItem.Enabled = enabled;
                else if (DoesItemMatchSearch(child, searchString))
                    childItem.Enabled = enabled;
            }

            ApplyChanges();
        }

        private void ToggleAll(bool enabled)
        {
            foreach (var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;
                if (searchString == null)
                    childItem.Enabled = enabled;
                else if (DoesItemMatchSearch(child, searchString))
                    childItem.Enabled = enabled;
            }

            ApplyChanges();
        }

        void ApplyChanges()
        {
            CoverageAnalytics.instance.CurrentCoverageEvent.updateAssembliesDialog = true;

            StringBuilder sb = new StringBuilder();
            foreach (var child in rootItem.children)
            {
                AssembliesTreeViewItem childItem = child as AssembliesTreeViewItem;
                if (childItem.Enabled)
                {
                    if (sb.Length > 0)
                        sb.Append(",");

                    sb.Append(childItem.displayName);
                }
            }

            m_Parent.AssembliesToInclude = sb.ToString();
            m_Parent.Repaint();
        }
    }

#if UNITY_6000_3_OR_NEWER
    class AssembliesTreeViewItem : TreeViewItem<int>
#else
    class AssembliesTreeViewItem : TreeViewItem
#endif
    {
        public bool Enabled { get; set; }
    }
}