
// This file has been generated by the GUI designer. Do not modify.
namespace CmisSync.Widgets
{
	public partial class ProxyWidget
	{
		private global::Gtk.VBox vbox3;
		private global::Gtk.RadioButton noProxyButton;
		private global::Gtk.RadioButton systemProxyButton;
		private global::Gtk.RadioButton customProxyButton;
		private global::Gtk.Label urlLabel;
		private global::CmisSync.Widgets.UrlWidget urlWidget;
		private global::Gtk.CheckButton credentialsRequiredButton;
		private global::Gtk.Table table1;
		private global::Gtk.Entry passwordEntry;
		private global::Gtk.Label passwordLabel;
		private global::Gtk.Entry userEntry;
		private global::Gtk.Label userLabel;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CmisSync.Widgets.ProxyWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CmisSync.Widgets.ProxyWidget";
			// Container child CmisSync.Widgets.ProxyWidget.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.noProxyButton = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("No Proxy"));
			this.noProxyButton.CanFocus = true;
			this.noProxyButton.Name = "noProxyButton";
			this.noProxyButton.DrawIndicator = true;
			this.noProxyButton.UseUnderline = true;
			this.noProxyButton.Group = new global::GLib.SList (global::System.IntPtr.Zero);
			this.vbox3.Add (this.noProxyButton);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.noProxyButton]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.systemProxyButton = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("System Default Proxy"));
			this.systemProxyButton.CanFocus = true;
			this.systemProxyButton.Name = "systemProxyButton";
			this.systemProxyButton.DrawIndicator = true;
			this.systemProxyButton.UseUnderline = true;
			this.systemProxyButton.Group = this.noProxyButton.Group;
			this.vbox3.Add (this.systemProxyButton);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.systemProxyButton]));
			w2.Position = 1;
			w2.Expand = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.customProxyButton = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("Custom Proxy"));
			this.customProxyButton.CanFocus = true;
			this.customProxyButton.Name = "customProxyButton";
			this.customProxyButton.DrawIndicator = true;
			this.customProxyButton.UseUnderline = true;
			this.customProxyButton.Group = this.noProxyButton.Group;
			this.vbox3.Add (this.customProxyButton);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.customProxyButton]));
			w3.Position = 2;
			w3.Expand = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.urlLabel = new global::Gtk.Label ();
			this.urlLabel.Name = "urlLabel";
			this.urlLabel.Xalign = 0F;
			this.urlLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Server");
			this.vbox3.Add (this.urlLabel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.urlLabel]));
			w4.Position = 3;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.urlWidget = new global::CmisSync.Widgets.UrlWidget ();
			this.urlWidget.Events = ((global::Gdk.EventMask)(256));
			this.urlWidget.Name = "urlWidget";
			this.urlWidget.IsUrlEditable = false;
			this.urlWidget.ValidationActivated = false;
			this.vbox3.Add (this.urlWidget);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.urlWidget]));
			w5.Position = 4;
			w5.Expand = false;
			w5.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.credentialsRequiredButton = new global::Gtk.CheckButton ();
			this.credentialsRequiredButton.CanFocus = true;
			this.credentialsRequiredButton.Name = "credentialsRequiredButton";
			this.credentialsRequiredButton.Label = global::Mono.Unix.Catalog.GetString ("Requires Authorization");
			this.credentialsRequiredButton.Active = true;
			this.credentialsRequiredButton.DrawIndicator = true;
			this.credentialsRequiredButton.UseUnderline = true;
			this.vbox3.Add (this.credentialsRequiredButton);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.credentialsRequiredButton]));
			w6.Position = 5;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(2)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.passwordEntry = new global::Gtk.Entry ();
			this.passwordEntry.CanFocus = true;
			this.passwordEntry.Name = "passwordEntry";
			this.passwordEntry.IsEditable = true;
			this.passwordEntry.InvisibleChar = '•';
			this.table1.Add (this.passwordEntry);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.passwordEntry]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(1));
			w7.RightAttach = ((uint)(2));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.passwordLabel = new global::Gtk.Label ();
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Xalign = 0F;
			this.passwordLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("Password");
			this.table1.Add (this.passwordLabel);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.passwordLabel]));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.userEntry = new global::Gtk.Entry ();
			this.userEntry.CanFocus = true;
			this.userEntry.Name = "userEntry";
			this.userEntry.IsEditable = true;
			this.userEntry.InvisibleChar = '•';
			this.table1.Add (this.userEntry);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.userEntry]));
			w9.TopAttach = ((uint)(1));
			w9.BottomAttach = ((uint)(2));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.userLabel = new global::Gtk.Label ();
			this.userLabel.Name = "userLabel";
			this.userLabel.Xalign = 0F;
			this.userLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("User");
			this.table1.Add (this.userLabel);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.userLabel]));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox3.Add (this.table1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.table1]));
			w11.Position = 6;
			w11.Expand = false;
			w11.Fill = false;
			this.Add (this.vbox3);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.noProxyButton.Activated += new global::System.EventHandler (this.OnNoProxyButtonActivated);
			this.systemProxyButton.Activated += new global::System.EventHandler (this.OnSystemProxyButtonActivated);
			this.customProxyButton.Activated += new global::System.EventHandler (this.OnCustomProxyButtonActivated);
		}
	}
}
