//   SparkleShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;

using SparkleLib;
using SparkleLib.Cmis;

namespace SparkleShare {

    public class SparkleSetup : SparkleSetupWindow {

        public SparkleSetupController Controller = new SparkleSetupController ();

        private ProgressBar progress_bar = new ProgressBar ();


        public SparkleSetup () : base ()
        {
            Controller.HideWindowEvent += delegate {
                Application.Invoke (delegate {
                    HideAll ();
                });
            };

            Controller.ShowWindowEvent += delegate {
                Application.Invoke (delegate {
                    ShowAll ();
                    Present ();
                });
            };

            Controller.ChangePageEvent += delegate (PageType type, string [] warnings) {
                Application.Invoke (delegate {
                    Reset ();

                    switch (type) {
                    case PageType.Setup: {

                        Header      = "Welcome to CmisSync!";
                        Description = "First off, what's your name and email?\nThis information is only visible to team members.";

                        Table table = new Table (2, 3, true) {
                            RowSpacing    = 6,
                            ColumnSpacing = 6
                        };

                            Label name_label = new Label ("<b>" + "Full Name:" + "</b>") {
                                UseMarkup = true,
                                Xalign    = 1
                            };

                            Entry name_entry = new Entry (UnixUserInfo.GetRealUser ().RealName) {
                                Xalign = 0,
                                ActivatesDefault = true
                            };

                            Entry email_entry = new Entry () {
                                Xalign = 0,
                                ActivatesDefault = true
                            };
                            
                            name_entry.Changed += delegate {
                                Controller.CheckSetupPage ();
                            };
                           
                            email_entry.Changed += delegate {
                                Controller.CheckSetupPage ();
                            };

                            Label email_label = new Label ("<b>" + "Email:" + "</b>") {
                                UseMarkup = true,
                                Xalign    = 1
                            };

                        table.Attach (name_label, 0, 1, 0, 1);
                        table.Attach (name_entry, 1, 2, 0, 1);
                        table.Attach (email_label, 0, 1, 1, 2);
                        table.Attach (email_entry, 1, 2, 1, 2);
                        
                        VBox wrapper = new VBox (false, 9);
                        wrapper.PackStart (table, true, false, 0);
                        
                            Button cancel_button = new Button ("Cancel");

                            cancel_button.Clicked += delegate {
                                Controller.SetupPageCancelled ();
                            };
                        
                            Button continue_button = new Button ("Continue") {
                                Sensitive = false
                            };

                            continue_button.Clicked += delegate (object o, EventArgs args) {
                                string full_name = name_entry.Text;
                                string email     = email_entry.Text;

                                Controller.SetupPageCompleted ();
                            };
                        
                        AddButton (cancel_button);
                        AddButton (continue_button);
                        Add (wrapper);


                        Controller.UpdateSetupContinueButtonEvent += delegate (bool button_enabled) {
                            Application.Invoke (delegate {
                                continue_button.Sensitive = button_enabled;
                            });
                        };

                        Controller.CheckSetupPage ();

                        break;
                    } 

                    case PageType.Add1: {

                        Header = "Where is your organization's server?";

                        VBox layout_vertical   = new VBox (false, 12);
                        HBox layout_fields     = new HBox (true, 12);
                        VBox layout_address    = new VBox (true, 0);
                        VBox layout_user       = new VBox (true, 0);
                        VBox layout_password   = new VBox (true, 0);

						
						// Address
                        Entry address_entry = new Entry () {
                            Text = Controller.PreviousAddress,
                            Sensitive = (Controller.SelectedPlugin.Address == null),
                            ActivatesDefault = true
                        };
                        
                        Label address_example = new Label () {
                            Xalign = 0,
                            UseMarkup = true,
                            Markup = "<span size=\"small\" fgcolor=\"" +
                                SecondaryTextColor + "\">" + Controller.SelectedPlugin.AddressExample + "</span>"
                        };

						Label address_help_label = new Label()
                        {
                            Text = "Help: ",
                            // TODO FontSize = 11,
                            // TODO Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128))
                        };
                        /* TODO Run run = new Run("Where to find this address");
                        Hyperlink link = new Hyperlink(run);
                        link.NavigateUri = new Uri("https://github.com/nicolas-raoul/CmisSync/wiki/What-address");
                        address_help_label.Inlines.Add(link);
                        link.RequestNavigate += (sender, e) =>
                            {
                                System.Diagnostics.Process.Start(e.Uri.ToString());
                            };*/

                        Label address_error_label = new Label()
                        {
                            // TODO FontSize = 11,
                            // TODO Foreground = new SolidColorBrush(Color.FromRgb(255, 128, 128)),
                            // TODO Visibility = Visibility.Hidden
                        };
						
						// User
                        Entry user_entry = new Entry () {
                            Text = Controller.PreviousPath,
                            Sensitive = (Controller.SelectedPlugin.User == null),
                            ActivatesDefault = true
                        };
                        
                        Label user_example = new Label () {
                            Xalign = 0,
                            UseMarkup = true,
                            Markup = "<span size=\"small\" fgcolor=\"" +
                                SecondaryTextColor + "\">" + Controller.SelectedPlugin.UserExample + "</span>"
                        };

						// Password
                        Entry password_entry = new Entry () {
                            Text = Controller.PreviousPath,
                            Sensitive = (Controller.SelectedPlugin.Password == null),
                            ActivatesDefault = true
                        };
                        
                        Label password_example = new Label () {
                            Xalign = 0,
                            UseMarkup = true,
                            Markup = "<span size=\"small\" fgcolor=\"" +
                                SecondaryTextColor + "\">" + Controller.SelectedPlugin.PasswordExample + "</span>"
                        };


                        Controller.ChangeAddressFieldEvent += delegate (string text,
                            string example_text, FieldState state) {

                            Application.Invoke (delegate {
                                address_entry.Text      = text;
                                address_entry.Sensitive = (state == FieldState.Enabled);
                                address_example.Markup  =  "<span size=\"small\" fgcolor=\"" +
                                    SecondaryTextColor + "\">" + example_text + "</span>";
                            });
                        };

                        Controller.ChangeUserFieldEvent += delegate (string text,
                            string example_text, FieldState state) {

                            Application.Invoke (delegate {
                                user_entry.Text      = text;
                                user_entry.Sensitive = (state == FieldState.Enabled);
                                user_example.Markup  =  "<span size=\"small\" fgcolor=\""
                                    + SecondaryTextColor + "\">" + example_text + "</span>";
                            });
                        };

                        Controller.ChangePasswordFieldEvent += delegate (string text,
                            string example_text, FieldState state) {

                            Application.Invoke (delegate {
                                password_entry.Text      = text;
                                password_entry.Sensitive = (state == FieldState.Enabled);
                                password_example.Markup  =  "<span size=\"small\" fgcolor=\""
                                    + SecondaryTextColor + "\">" + example_text + "</span>";
                            });
                        };

                        Controller.CheckAddPage (address_entry.Text);
                        
                        address_entry.Changed += delegate {
                            Controller.CheckAddPage (address_entry.Text);
                        };

						// Address
                        layout_address.PackStart (new Label () {
                                Markup = "<b>" + "Address:" + "</b>",
                                Xalign = 0
                            }, true, true, 0);

                        layout_address.PackStart (address_entry, false, false, 0);
                        layout_address.PackStart (address_example, false, false, 0);

						// User
						layout_user.PackStart (new Label () {
                                Markup = "<b>" + "User:" + "</b>",
                                Xalign = 0
                            }, true, true, 0);
                        layout_user.PackStart (user_entry, false, false, 0);
                        layout_user.PackStart (user_example, false, false, 0);

						// Password
						layout_password.PackStart (new Label () {
                                Markup = "<b>" + "password:" + "</b>",
                                Xalign = 0
                            }, true, true, 0);
                        layout_password.PackStart (password_entry, false, false, 0);
                        layout_password.PackStart (password_example, false, false, 0);

                        layout_fields.PackStart (layout_address);
                        layout_fields.PackStart (layout_user);
                        layout_fields.PackStart (layout_password);

                        layout_vertical.PackStart (new Label (""), false, false, 0);
                        layout_vertical.PackStart (layout_fields, false, false, 0);

                        Add (layout_vertical);

                            // Cancel button
                            Button cancel_button = new Button ("Cancel");

                            cancel_button.Clicked += delegate {
                                Controller.PageCancelled ();
                            };
							
							// Continue button
                            Button continue_button = new Button ("Continue") {
                                Sensitive = false
                            };

                            continue_button.Clicked += delegate {
	                            // Show wait cursor
	                            // TODO System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
	
								SparkleLogger.LogInfo("SparkleSetup", "address:" + address_entry.Text + " user:" + user_entry.Text + " password:" + password_entry.Text);
							
	                            // Try to find the CMIS server
	                            CmisServer cmisServer = CmisUtils.GetRepositoriesFuzzy(
	                                address_entry.Text, user_entry.Text, password_entry.Text);
	                            Controller.repositories = cmisServer.repositories;
	                            address_entry.Text = cmisServer.url;
	
	                            // Hide wait cursor
	                            // TODO System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
	
	                            if (Controller.repositories == null)
	                            {
	                                // Show warning
	                                address_error_label.Text = "Sorry, CmisSync can not find a CMIS server at this address.\nPlease check again.\nIf you are sure about the address, open it in a browser and post\nthe resulting XML to the CmisSync forum.";
	                                // TODO address_error_label.Visibility = Visibility.Visible;
	                            }
	                            else
	                            {
									SparkleLogger.LogInfo("SparkleSetup", "repositories[0]:" + Controller.repositories[0]);
	                                // Continue to folder selection
	                                Controller.Add1PageCompleted(
	                                    address_entry.Text, user_entry.Text, password_entry.Text);
	                            }
	                        };


                        Controller.UpdateAddProjectButtonEvent += delegate (bool button_enabled) {
                            Application.Invoke (delegate {
                                continue_button.Sensitive = button_enabled;                            
                            });
                        };
                        
                        AddButton (cancel_button);
                        AddButton (continue_button);
                        
                        Controller.CheckAddPage (address_entry.Text);

                        break;
                    }

                    case PageType.Add2: {

                        Header = "Which remote folder do you want to sync?";

                        VBox layout_vertical   = new VBox (false, 12);
                        HBox layout_fields     = new HBox (true, 12);
                        VBox layout_repository = new VBox (true, 0);
                        VBox layout_path       = new VBox (true, 0);

						// Repository
                        Entry repository_entry = new Entry () {
                            Text = Controller.repositories[0], // TODO put all elements in a tree
                            Sensitive = (Controller.SelectedPlugin.Repository == null),
                            ActivatesDefault = true
                        };
                        
                        Label repository_example = new Label () {
                            Xalign = 0,
                            UseMarkup = true,
                            Markup = "<span size=\"small\" fgcolor=\"" +
                                SecondaryTextColor + "\">" + Controller.SelectedPlugin.RepositoryExample + "</span>"
                        };

						// Path
                        Entry path_entry = new Entry () {
                            Text = "/",
                            Sensitive = (Controller.SelectedPlugin.Path == null),
                            ActivatesDefault = true
                        };
                        
                        Label path_example = new Label () {
                            Xalign = 0,
                            UseMarkup = true,
                            Markup = "<span size=\"small\" fgcolor=\"" +
                                SecondaryTextColor + "\">" + Controller.SelectedPlugin.PathExample + "</span>"
                        };

                        Controller.ChangeRepositoryFieldEvent += delegate (string text,
                            string example_text, FieldState state) {

                            Application.Invoke (delegate {
                                repository_entry.Text      = text;
                                repository_entry.Sensitive = (state == FieldState.Enabled);
                                repository_example.Markup  =  "<span size=\"small\" fgcolor=\"" +
                                    SecondaryTextColor + "\">" + example_text + "</span>";
                            });
                        };

                        Controller.ChangePathFieldEvent += delegate (string text,
                            string example_text, FieldState state) {

                            Application.Invoke (delegate {
                                path_entry.Text      = text;
                                path_entry.Sensitive = (state == FieldState.Enabled);
                                path_example.Markup  =  "<span size=\"small\" fgcolor=\""
                                    + SecondaryTextColor + "\">" + example_text + "</span>";
                            });
                        };

                        //Controller.CheckAddPage (address_entry.Text);
                        
						// Repository
						layout_repository.PackStart (new Label () {
                                Markup = "<b>" + "Repository:" + "</b>",
                                Xalign = 0
                            }, true, true, 0);
                        layout_repository.PackStart (repository_entry, false, false, 0);
                        layout_repository.PackStart (repository_example, false, false, 0);

						// Path
						layout_path.PackStart (new Label () {
                                Markup = "<b>" + "Remote Path:" + "</b>",
                                Xalign = 0
                            }, true, true, 0);
                        layout_path.PackStart (path_entry, false, false, 0);
                        layout_path.PackStart (path_example, false, false, 0);

                        layout_fields.PackStart (layout_repository);
                        layout_fields.PackStart (layout_path);

                        layout_vertical.PackStart (new Label (""), false, false, 0);
                        layout_vertical.PackStart (layout_fields, false, false, 0);

                        Add (layout_vertical);

                        // Cancel button
                        Button cancel_button = new Button ("Cancel");

                        cancel_button.Clicked += delegate {
                            Controller.PageCancelled ();
                        };

                        Button add_button = new Button ("Add") {
                            //Sensitive = false
                        };

                        add_button.Clicked += delegate {
                            Controller.Add2PageCompleted(repository_entry.Text, path_entry.Text);
                        };

                        Controller.UpdateAddProjectButtonEvent += delegate (bool button_enabled) {
                            Application.Invoke (delegate {
                                add_button.Sensitive = button_enabled;                            
                            });
                        };
                        
                        AddButton (cancel_button);
                        AddButton (add_button);
                        
                        //Controller.CheckAddPage (address_entry.Text);

                        break;
                    }

                    case PageType.Invite: {

                        Header      = "You've received an invite!";
                        Description = "Do you want to add this project to SparkleShare?";


                        Table table = new Table (2, 3, true) {
                            RowSpacing    = 6,
                            ColumnSpacing = 6
                        };

                            Label address_label = new Label ("Address:") {
                                Xalign    = 1
                            };

                            Label path_label = new Label ("Remote Path:") {
                                Xalign    = 1
                            };

                            Label address_value = new Label ("<b>" + Controller.PendingInvite.Address + "</b>") {
                                UseMarkup = true,
                                Xalign    = 0
                            };

                            Label path_value = new Label ("<b>" + Controller.PendingInvite.RemotePath + "</b>") {
                                UseMarkup = true,
                                Xalign    = 0
                            };

                        table.Attach (address_label, 0, 1, 0, 1);
                        table.Attach (address_value, 1, 2, 0, 1);
                        table.Attach (path_label, 0, 1, 1, 2);
                        table.Attach (path_value, 1, 2, 1, 2);

                        VBox wrapper = new VBox (false, 9);
                        wrapper.PackStart (table, true, false, 0);

                            Button cancel_button = new Button ("Cancel");

                            cancel_button.Clicked += delegate {
                                Controller.PageCancelled ();
                            };

                            Button add_button = new Button ("Add");

                            add_button.Clicked += delegate {
                                Controller.InvitePageCompleted ();
                            };

                        AddButton (cancel_button);
                        AddButton (add_button);
                        Add (wrapper);

                        break;
                    }

                    case PageType.Syncing: {

                        Header      = String.Format ("Adding project ‘{0}’…", Controller.SyncingFolder);
                        Description = "This may either take a short or a long time depending on the project's size.";

                        this.progress_bar.Fraction = Controller.ProgressBarPercentage / 100;

                        Button finish_button = new Button () {
                            Sensitive = false,
                            Label = "Finish"
                        };

                        Button cancel_button = new Button () {
                            Label = "Cancel"
                        };

                        cancel_button.Clicked += delegate {
                            Controller.SyncingCancelled ();
                        };

                        AddButton (cancel_button);
                        AddButton (finish_button);

                        Controller.UpdateProgressBarEvent += delegate (double percentage) {
                            Application.Invoke (delegate {
                                this.progress_bar.Fraction = percentage / 100;
                            });
                        };

                        if (this.progress_bar.Parent != null)
                           (this.progress_bar.Parent as Container).Remove (this.progress_bar);

                        VBox bar_wrapper = new VBox (false, 0);
                        bar_wrapper.PackStart (this.progress_bar, false, false, 15);

                        Add (bar_wrapper);

                        break;
                    }

                    case PageType.Error: {
                    
                        Header = "Oops! Something went wrong" + "…";

                        VBox points = new VBox (false, 0);
                        Image list_point_one   = new Image (SparkleUIHelpers.GetIcon ("go-next", 16));
                        Image list_point_two   = new Image (SparkleUIHelpers.GetIcon ("go-next", 16));
                        Image list_point_three = new Image (SparkleUIHelpers.GetIcon ("go-next", 16));

                        Label label_one = new Label () {
                            Markup = "<b>" + Controller.PreviousUrl + "</b> is the address we've compiled. " +
                            "Does this look alright?",
                            Wrap   = true,
                            Xalign = 0
                        };

                        Label label_two = new Label () {
                            Text   = "Do you have access rights to this remote project?",
                            Wrap   = true,
                            Xalign = 0
                        };
                        
                        points.PackStart (new Label ("Please check the following:") { Xalign = 0 }, false, false, 6);

                        HBox point_one = new HBox (false, 0);
                        point_one.PackStart (list_point_one, false, false, 0);
                        point_one.PackStart (label_one, true, true, 12);
                        points.PackStart (point_one, false, false, 12);
                        
                        HBox point_two = new HBox (false, 0);
                        point_two.PackStart (list_point_two, false, false, 0);
                        point_two.PackStart (label_two, true, true, 12);
                        points.PackStart (point_two, false, false, 12);

                        if (warnings.Length > 0) {
                            string warnings_markup = "";

                            foreach (string warning in warnings)
                                warnings_markup += "\n<b>" + warning + "</b>";

                            Label label_three = new Label () {
                                Markup = "Here's the raw error message:" + warnings_markup,
                                Wrap   = true,
                                Xalign = 0
                            };

                            HBox point_three = new HBox (false, 0);
                            point_three.PackStart (list_point_three, false, false, 0);
                            point_three.PackStart (label_three, true, true, 12);
                            points.PackStart (point_three, false, false, 12);
                        }

                        points.PackStart (new Label (""), true, true, 0);

                        Button cancel_button = new Button ("Cancel");

                            cancel_button.Clicked += delegate {
                                Controller.PageCancelled ();
                            };
                        
                        Button try_again_button = new Button ("Try Again…") {
                            Sensitive = true
                        };

                        try_again_button.Clicked += delegate {
                            Controller.ErrorPageCompleted ();
                        };
                        
                        AddButton (cancel_button);
                        AddButton (try_again_button);
                        Add (points);

                        break;
                    }
    
                    case PageType.CryptoSetup: {

                        Header       = "Set up file encryption";
                        Description  = "This project is supposed to be encrypted, but it doesn't yet have a password set. Please provide one below.";
                        
                        Label password_label = new Label ("<b>" + "Password:" + "</b>") {
                            UseMarkup = true,
                            Xalign    = 1
                        };

                        Entry password_entry = new Entry () {
                            Xalign = 0,
                            Visibility = false,
                            ActivatesDefault = true
                        };
                        
                        CheckButton show_password_check_button = new CheckButton ("Show password") {
                            Active = false,
                            Xalign = 0,
                        };
                        
                        show_password_check_button.Toggled += delegate {
                            password_entry.Visibility = !password_entry.Visibility;
                        };

                        password_entry.Changed += delegate {
                            Controller.CheckCryptoSetupPage (password_entry.Text);
                        };
                         

                        Button continue_button = new Button ("Continue") {
                            Sensitive = false
                        };

                        continue_button.Clicked += delegate {
                           Controller.CryptoSetupPageCompleted (password_entry.Text);
                        };

                        Button cancel_button = new Button ("Cancel");

                        cancel_button.Clicked += delegate {
                            Controller.CryptoPageCancelled ();
                        };

                        Controller.UpdateCryptoSetupContinueButtonEvent += delegate (bool button_enabled) {
                            Application.Invoke (delegate {
                                continue_button.Sensitive = button_enabled;
                            });
                        };
                        
                        
                        Table table = new Table (2, 3, true) {
                            RowSpacing    = 6,
                            ColumnSpacing = 6
                        };


                        table.Attach (password_label, 0, 1, 0, 1);
                        table.Attach (password_entry, 1, 2, 0, 1);
                        
                        table.Attach (show_password_check_button, 1, 2, 1, 2);
                        
                        VBox wrapper = new VBox (false, 9);
                        wrapper.PackStart (table, true, false, 0);

                        
                        Image warning_image = new Image (
                            SparkleUIHelpers.GetIcon ("dialog-information", 24)
                        );

                        Label warning_label = new Label () {
                            Xalign = 0,
                            Wrap   = true,
                            Text   = "This password can't be changed later, and your files can't be recovered if it's forgotten."
                        };

                        HBox warning_layout = new HBox (false, 0);
                        warning_layout.PackStart (warning_image, false, false, 15);
                        warning_layout.PackStart (warning_label, true, true, 0);
                        
                        VBox warning_wrapper = new VBox (false, 0);
                        warning_wrapper.PackStart (warning_layout, false, false, 15);

                        wrapper.PackStart (warning_wrapper, false, false, 0);
                    
                        
                        Add (wrapper);



                        AddButton (cancel_button);
                        AddButton (continue_button);

                        break;
                    }

                    case PageType.CryptoPassword: {

                        Header       = "This project contains encrypted files";
                        Description  = "Please enter the password to see their contents.";
                        
                        Label password_label = new Label ("<b>" + "Password:" + "</b>") {
                            UseMarkup = true,
                            Xalign    = 1
                        };

                        Entry password_entry = new Entry () {
                            Xalign = 0,
                            Visibility = false,
                            ActivatesDefault = true
                        };
                        
                        CheckButton show_password_check_button = new CheckButton ("Show password") {
                            Active = false,
                            Xalign = 0
                        };
                        
                        show_password_check_button.Toggled += delegate {
                            password_entry.Visibility = !password_entry.Visibility;
                        };

                        password_entry.Changed += delegate {
                            Controller.CheckCryptoPasswordPage (password_entry.Text);
                        };
                         

                        Button continue_button = new Button ("Continue") {
                            Sensitive = false
                        };

                        continue_button.Clicked += delegate {
                           Controller.CryptoPasswordPageCompleted (password_entry.Text);
                        };

                        Button cancel_button = new Button ("Cancel");

                        cancel_button.Clicked += delegate {
                            Controller.CryptoPageCancelled ();
                        };

                        Controller.UpdateCryptoPasswordContinueButtonEvent += delegate (bool button_enabled) {
                            Application.Invoke (delegate {
                                continue_button.Sensitive = button_enabled;
                            });
                        };
                        
                        Table table = new Table (2, 3, true) {
                            RowSpacing    = 6,
                            ColumnSpacing = 6
                        };

                        table.Attach (password_label, 0, 1, 0, 1);
                        table.Attach (password_entry, 1, 2, 0, 1);
                        
                        table.Attach (show_password_check_button, 1, 2, 1, 2);
                        
                        VBox wrapper = new VBox (false, 9);
                        wrapper.PackStart (table, true, false, 0);

                        Add (wrapper);

                        AddButton (cancel_button);
                        AddButton (continue_button);

                        break;
                    }
                        
                    case PageType.Finished: {

                        UrgencyHint = true;

                        if (!HasToplevelFocus) {
                            string title   = "Your documents are ready!";
                            string subtext = "You can find them in your CmisSync folder.";

                            Program.UI.Bubbles.Controller.ShowBubble (title, subtext, null);
                        }

                        Header      = "Your documents are ready!";
                        Description = "You can find them in your CmisSync folder.";

                        // A button that opens the synced folder
                        Button open_folder_button = new Button (string.Format ("Open {0}",
                            System.IO.Path.GetFileName (Controller.PreviousPath)));

                        open_folder_button.Clicked += delegate {
                            Controller.OpenFolderClicked ();
                        };

                        Button finish_button = new Button ("Finish");

                        finish_button.Clicked += delegate {
                            Controller.FinishPageCompleted ();
                        };


                        if (warnings.Length > 0) {
                            Image warning_image = new Image (
                                SparkleUIHelpers.GetIcon ("dialog-information", 24)
                            );

                            Label warning_label = new Label (warnings [0]) {
                                Xalign = 0,
                                Wrap   = true
                            };

                            HBox warning_layout = new HBox (false, 0);
                            warning_layout.PackStart (warning_image, false, false, 15);
                            warning_layout.PackStart (warning_label, true, true, 0);
                            
                            VBox warning_wrapper = new VBox (false, 0);
                            warning_wrapper.PackStart (warning_layout, false, false, 0);

                            Add (warning_wrapper);

                        } else {
                            Add (null);
                        }


                        AddButton (open_folder_button);
                        AddButton (finish_button);

                        break;
                    }


                    case PageType.Tutorial: {

                        switch (Controller.TutorialPageNumber) {
                        case 1: {
                            Header      = "What's happening next?";
                            Description = "CmisSync creates a special folder on your computer " +
                                "that will keep track of your folders.";

                            Button skip_tutorial_button = new Button ("Skip Tutorial");
                            skip_tutorial_button.Clicked += delegate {
                                Controller.TutorialSkipped ();
                            };

                            Button continue_button = new Button ("Continue");
                            continue_button.Clicked += delegate {
                                Controller.TutorialPageCompleted ();
                            };

                            Image slide = SparkleUIHelpers.GetImage ("tutorial-slide-1.png");

                            Add (slide);

                            AddButton (skip_tutorial_button);
                            AddButton (continue_button);

                            break;
                        }

                        case 2: {
                            Header      = "Sharing files with others";
                            Description = "All files added to the server are automatically synced to your " +
                                "local folder.";

                            Button continue_button = new Button ("Continue");
                            continue_button.Clicked += delegate {
                                Controller.TutorialPageCompleted ();
                            };

                            Image slide = SparkleUIHelpers.GetImage ("tutorial-slide-2.png");

                            Add (slide);
                            AddButton (continue_button);

                            break;
                        }

                        case 3: {
                            Header      = "The status icon is here to help";
                            Description = "It shows the syncing progress, provides easy access to " +
                                "your folders and let's you view recent changes.";

                            Button continue_button = new Button ("Continue");
                            continue_button.Clicked += delegate {
                                Controller.TutorialPageCompleted ();
                            };

                            Image slide = SparkleUIHelpers.GetImage ("tutorial-slide-3.png");

                            Add (slide);
                            AddButton (continue_button);

                            break;
                        }

                        case 4: {
                            Header      = "Adding repository folders to CmisSync";
                            Description = "           " +
                                "           ";

                            Image slide = SparkleUIHelpers.GetImage ("tutorial-slide-4.png");

                            Button finish_button = new Button ("Finish");
                            finish_button.Clicked += delegate {
                                Controller.TutorialPageCompleted ();
                            };


                            CheckButton check_button = new CheckButton ("Add CmisSync to startup items") {
                                Active = true
                            };

                            check_button.Toggled += delegate {
                                Controller.StartupItemChanged (check_button.Active);
                            };

                            Add (slide);
                            AddOption (check_button);
                            AddButton (finish_button);

                            break;
                        }
                        }

                        break;
                    }
                    }

                    ShowAll ();
                });
            };
        }

    
        private void RenderServiceColumn (TreeViewColumn column, CellRenderer cell,
            TreeModel model, TreeIter iter)
        {
            string markup           = (string) model.GetValue (iter, 1);
            TreeSelection selection = (column.TreeView as TreeView).Selection;

            if (selection.IterIsSelected (iter)) {
                if (column.TreeView.HasFocus)
                    markup = markup.Replace (SecondaryTextColor, SecondaryTextColorSelected);
                else
                    markup = markup.Replace (SecondaryTextColorSelected, SecondaryTextColor);
            } else {
                markup = markup.Replace (SecondaryTextColorSelected, SecondaryTextColor);
            }

            (cell as CellRendererText).Markup = markup;
        }
    }


    public class SparkleTreeView : TreeView {

        public int SelectedRow
        {
            get {
                TreeIter iter;
                TreeModel model;

                Selection.GetSelected (out model, out iter);

                return int.Parse (model.GetPath (iter).ToString ());
            }
        }


        public SparkleTreeView (ListStore store) : base (store)
        {
        }
    }
}