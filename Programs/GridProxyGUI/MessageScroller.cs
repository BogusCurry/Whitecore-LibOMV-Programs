﻿using System;
using System.Collections.Generic;
using Gtk;

namespace GridProxyGUI
{
    internal class MessageScroller : TreeView
    {
        public static string[] ColumnLabels = { "Nr", "Timestamp", "Protocol", "Type", "Size", "URL", "Content Type" };
        Dictionary<string, int> ColumnMap = new Dictionary<string, int>();

        public ListStore Messages;
        public bool AutoScroll = true;

        public MessageScroller()
        {

            for (int i = 0; i < ColumnLabels.Length; i++)
            {
                CellRendererText cell = new CellRendererText();
                TreeViewColumn col = new TreeViewColumn();
                col.PackStart(cell, true);
                col.SetCellDataFunc(cell, CellDataFunc);
                col.Title = ColumnLabels[i];
                AppendColumn(col);
                ColumnMap[ColumnLabels[i]] = i;
            }

            Model = Messages = new ListStore(typeof(Session));
            HeadersVisible = true;
            Selection.Mode = SelectionMode.Multiple;
            ShowAll();
        }

        protected void ShowContext()
        {
            var selected = Selection.GetSelectedRows();
            if (selected.Length < 1) return;

            Menu context = new Menu();

            MenuItem item = new MenuItem("Remove");
            item.Activated += (sender, e) =>
            {
                foreach (var p in selected)
                {
                    TreeIter iter;
                    if (Messages.GetIter(out iter, p))
                    {
                        Messages.Remove(ref iter);
                    }
                }
                Selection.UnselectAll();
            };
            context.Add(item);

            item = new MenuItem("Remove All");
            item.Activated += (sender, e) =>
            {
                Selection.UnselectAll();
                Messages.Clear();
            };
            context.Add(item);

            context.Add(new SeparatorMenuItem());

            item = new MenuItem("Select All");
            item.Activated += (sender, e) =>
            {
                Selection.SelectAll();
            };
            context.Add(item);

            item = new MenuItem("Deselect");
            item.Activated += (sender, e) =>
            {
                Selection.UnselectAll();
            };
            context.Add(item);

            context.Add(new SeparatorMenuItem());

            item = new MenuItem("Copy");
            item.Activated += (sender, e) =>
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var p in selected)
                {
                    TreeIter iter;
                    if (Messages.GetIter(out iter, p))
                    {
                        var session = Messages.GetValue(iter, 0) as Session;
                        if (session != null)
                        {
                            sb.AppendLine(string.Join(" | ", session.Columns));
                        }
                    }

                    Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false)).Text = sb.ToString().TrimEnd();
                }
            };
            context.Add(item);

            context.ShowAll();
            context.Popup();
        }

        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            if (evnt.Type == Gdk.EventType.ButtonPress && evnt.Button == 3) // right click press
            {
                TreePath path;
                if (GetPathAtPos((int)evnt.X, (int)evnt.Y, out path))
                {
                    bool amISelected = false;
                    foreach (var p in Selection.GetSelectedRows())
                    {
                        if (p.Compare(path) == 0)
                        {
                            amISelected = true;
                            break;
                        }
                    }

                    if (!amISelected)
                    {
                        Selection.UnselectAll();
                        Selection.SelectPath(path);
                    }
                }

                ShowContext();

                return true;
            }

            return base.OnButtonPressEvent(evnt);
        }

        void CellDataFunc(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            var item = model.GetValue(iter, 0) as Session;
            if (item != null)
            {
                if (ColumnMap.ContainsKey(column.Title))
                {
                    int pos = ColumnMap[column.Title];
                    if (pos < item.Columns.Length)
                    {
                        ((CellRendererText)cell).Text = item.Columns[pos];
                    }
                }
            }
        }

        public void AddSession(Session s)
        {
            TreeIter iter = Messages.AppendValues(s);
            if (AutoScroll)
            {
                ScrollToCell(Messages.GetPath(iter), null, true, 0, 0);
            }
        }
    }
}
