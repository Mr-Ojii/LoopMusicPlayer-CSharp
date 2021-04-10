using Gtk;
using ManagedBass;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UI = Gtk.Builder.ObjectAttribute;

namespace LoopMusicPlayer
{
    internal class MainWindow : Window
    {
        [UI] private Button _playbutton = null;
        [UI] private Button _pausebutton = null;
        [UI] private Button _stopbutton = null;
        [UI] private Button _previousbutton = null;
        [UI] private Button _nextbutton = null;
        [UI] private Button _ejectbutton = null;
        [UI] private VolumeButton _volumebutton = null;
        [UI] private TreeView _treeview = null;
        [UI] private ListStore _liststore = null;

        [UI] private TreeViewColumn _titlecolumn = null;
        [UI] private TreeViewColumn _timecolumn = null;
        [UI] private TreeViewColumn _loopcolumn = null;
        [UI] private TreeViewColumn _artistcolumn = null;
        [UI] private TreeViewColumn _pathcolumn = null;
        private Gtk.CellRendererText TitleNameCell = null;
        private Gtk.CellRendererText TimeCell = null;
        private Gtk.CellRendererText LoopCell = null;
        private Gtk.CellRendererText ArtistNameCell = null;
        private Gtk.CellRendererText PathCell = null;

        [UI] private ImageMenuItem _listaddmenu = null;
        [UI] private ImageMenuItem _listdeletemenu = null;
        [UI] private ImageMenuItem _listclearmenu = null;
        [UI] private ImageMenuItem _quitmenu = null;

        [UI] private CheckMenuItem _showgridlinemenu = null;
        [UI] private RadioMenuItem _labelelpsedtimemenu = null;
        [UI] private RadioMenuItem _labelseektimemenu = null;
        [UI] private RadioMenuItem _labelremainingtimemenu = null;
        [UI] private CheckMenuItem _windowkeepabovemenu = null;

        [UI] private ImageMenuItem _aboutmenu = null;

        [UI] private DrawingArea _seekbararea = null;

        [UI] private Label _labeltitle = null;
        [UI] private Label _labelpath = null;
        [UI] private Label _labelnowtime = null;
        [UI] private Label _labellooptime = null;

        private Player player
        {
            set
            {
                _player?.Dispose();
                _player = value;
            }
            get
            {
                return _player;
            }
        }

        private Player _player = null;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            Bass.Init();

            Bass.Configure(Configuration.UpdatePeriod, 1);
            Bass.Configure(Configuration.PlaybackBufferLength, 50);

            builder.Autoconnect(this);

            _seekbararea.AddTickCallback(onframetick);

            TitleNameCell = new Gtk.CellRendererText();
            TimeCell = new Gtk.CellRendererText();
            LoopCell = new Gtk.CellRendererText();
            ArtistNameCell = new Gtk.CellRendererText();
            PathCell = new Gtk.CellRendererText();

            _titlecolumn.PackStart(TitleNameCell, true);
            _timecolumn.PackStart(TimeCell, true);
            _loopcolumn.PackStart(LoopCell, true);
            _artistcolumn.PackStart(ArtistNameCell, true);
            _pathcolumn.PackStart(PathCell, true);

            _titlecolumn.AddAttribute(TitleNameCell, "text", 0);
            _timecolumn.AddAttribute(TimeCell, "text", 1);
            _loopcolumn.AddAttribute(LoopCell, "text", 2);
            _artistcolumn.AddAttribute(ArtistNameCell, "text", 3);
            _pathcolumn.AddAttribute(PathCell, "text", 4);

            DeleteEvent += Window_DeleteEvent;

            var targets = new[] {
                new TargetEntry("text/uri-list",TargetFlags.OtherApp,0)
            };
            Drag.DestSet(this, DestDefaults.All, targets, Gdk.DragAction.Copy | Gdk.DragAction.Move);
            DragDataReceived += TreeViewDragDataReceived;

            _labelseektimemenu.Toggle();
            _aboutmenu.Activated += ShowAbout;
            _listaddmenu.Activated += OpenFileFromMenu;
            _seekbararea.Drawn += DrawingArea_OnDraw;
            _quitmenu.Activated += WindowQuit;
            _listclearmenu.Activated += ListClear;
            _listdeletemenu.Activated += ListDelete;
            _treeview.RowActivated += ActivateLow;
            _volumebutton.ValueChanged += VolumeChanged;
            _pausebutton.Clicked += PauseClicked;
            _stopbutton.Clicked += StopClicked;
            _playbutton.Clicked += PlayClicked;
            _previousbutton.Clicked += PreviousClicked;
            _nextbutton.Clicked += NextClicked;
            _ejectbutton.Clicked += EjectClicked;
            _seekbararea.AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _seekbararea.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
            _seekbararea.ButtonPressEvent += SeekBarButtonPress;
            _seekbararea.ButtonReleaseEvent += SeekBarButtonRelease;
            _windowkeepabovemenu.Toggled += WindowAboveToggled;
            _showgridlinemenu.Toggled += ShowGridMenuToggled;

        }


        private void EjectClicked(object o, EventArgs args)
        {
            this.player?.Dispose();
            this.player = null;

            this._labeltitle.Text = "";
            this._labelpath.Text = "";
            this._labellooptime.Text = "";
            this._labelnowtime.Text = "";
        }

        private void ShowGridMenuToggled(object o, EventArgs args)
        {
            this._treeview.EnableGridLines = _showgridlinemenu.Active ? TreeViewGridLines.Both : TreeViewGridLines.None;
        }

        private void WindowAboveToggled(object o, EventArgs args) 
        {
            this.KeepAbove = _windowkeepabovemenu.Active;
        }

        private void CreatePlayer(string path)
        {
            try
            {
                this.player = new Player(path, _volumebutton.Value);
            }catch(Exception e)
            {
                Trace.TraceError(e.ToString());
                this.player?.Dispose();
                this.player = null;

                this._labeltitle.Text = "Error occurred while loading the file.";
                this._labelpath.Text = "";
                this._labellooptime.Text = "";
                this._labelnowtime.Text = "";

                return;
            }
            this._labeltitle.Text = this.player.Title;
            this._labelpath.Text = this.player.FilePath;
            this._labellooptime.Text = "Looptime: " + this.player.LoopStartTime.ToString(@"hh\:mm\:ss\.ff") + " - " + this.player.LoopEndTime.ToString(@"hh\:mm\:ss\.ff");
        }

        private void SeekBarButtonRelease(object o, ButtonReleaseEventArgs args)
        {
            if (this.player != null)
            {
                DrawingArea area = o as DrawingArea;
                int clickedx = (int)Math.Max(Math.Min(args.Event.X - 5, (area.AllocatedWidth - 10)), 0);

                double ratio = clickedx / (double)(area.AllocatedWidth - 10);

                this.player.Seek((long)(ratio * this.player.TotalSamples));
            }
        }

        private void SeekBarButtonPress(object o, ButtonPressEventArgs args)
        {

        }

        private void PreviousClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.IterPrevious(ref iter);
                _treeview.Selection.SelectIter(iter);
            }
        }

        private void NextClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.IterNext(ref iter);
                _treeview.Selection.SelectIter(iter);
            }
        }

        private void PlayClicked(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                string path = _treeview.Model.GetValue(iter, 4) as string;
                CreatePlayer(path);
            }

            this.player?.Play();
        }

        private void StopClicked(object o, EventArgs args) 
        {
            this.player?.Stop();
        }

        private void PauseClicked(object o, EventArgs args)
        {
            this.player?.Pause();
        }

        private void VolumeChanged(object o, ValueChangedArgs args) 
        {
            player?.ChangeVolume(_volumebutton.Value);
        }

        private bool onframetick(Widget widget, Gdk.FrameClock frame_clock)
        {
            if (this.player != null)
            {
                if (this._labelseektimemenu.Active)
                    this._labelnowtime.Text = this.player.TimePosition.ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
                else if (this._labelelpsedtimemenu.Active)
                    this._labelnowtime.Text = "+" + (this.player.LoopCount * (this.player.LoopEndTime - this.player.LoopStartTime) + this.player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
                else if (this._labelremainingtimemenu.Active)
                    this._labelnowtime.Text = "-" + (this.player.TotalTime - this.player.TimePosition).ToString(@"hh\:mm\:ss\.ff") + " / " + this.player.TotalTime.ToString(@"hh\:mm\:ss\.ff");
            }
            _seekbararea.QueueDraw();
            return true;
        }

        private void TreeViewDragDataReceived(object o, DragDataReceivedArgs args)
        {
            if (args.SelectionData.Length > 0)
            {
                string motofiles = System.Text.Encoding.UTF8.GetString(args.SelectionData.Data);
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                string files = System.Web.HttpUtility.UrlDecode(motofiles, enc);
                files = files.Replace("\0", "");
                files = files.Replace("\r", "");
                string[] fileArray = files.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fileArray.Length; i++)
                {
                    if (fileArray[i].StartsWith("file://"))
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            fileArray[i] = fileArray[i].Substring(8);
                        }
                        else
                        {
                            fileArray[i] = fileArray[i].Substring(7);
                        }
                }
                AddSongs(fileArray);
            }
        }

        private void ActivateLow(object o, RowActivatedArgs args)
        {
            _treeview.Model.GetIter(out TreeIter iter, args.Path);
            string path = _treeview.Model.GetValue(iter, 4) as string;

            CreatePlayer(path);
        }

        private void ListClear(object o, EventArgs args)
        {
            _liststore.Clear();
        }

        private void ListDelete(object o, EventArgs args)
        {
            if (_treeview.Selection.GetSelected(out var iter))
            {
                _liststore.Remove(ref iter);
            }
        }

        private void WindowQuit(object o, EventArgs args)
        {
            Close();
        }

        private void DrawingArea_OnDraw(object o, DrawnArgs args)
        {
            Widget widget = o as Widget;
            Cairo.Context cr = args.Cr;
            cr.SetSourceRGB(0.9, 0.9, 0.9);
            cr.Rectangle(0, 0, widget.Allocation.Width, widget.Allocation.Height);

            cr.Fill();

            cr.SetSourceRGB(1.0, 1.0, 1.0);
            cr.Rectangle(5, 5, widget.Allocation.Width - 10, widget.Allocation.Height - 10);

            cr.Fill();

            if (this.player != null) 
            {
                if (this.player.IsLoop) 
                {
                    cr.SetSourceRGB(0.21, 0.517, 0.894);
                    cr.Rectangle((int)((widget.Allocation.Width - 10) * ((double)this.player.LoopStart / player.TotalSamples)) + 5, 5, (int)((widget.Allocation.Width - 10) * ((double)(this.player.LoopEnd - this.player.LoopStart) / player.TotalSamples)), widget.Allocation.Height - 10);
                    cr.Fill();
                }
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(((widget.Allocation.Width - 10) * ((double)player.SamplePosition / player.TotalSamples)), 0, 10, widget.Allocation.Height);
                cr.Fill();

                cr.SetSourceRGB(1.0, 1.0, 1.0);
                cr.Rectangle(((widget.Allocation.Width - 10) * ((double)player.SamplePosition / player.TotalSamples)) + 1, 1, 8, widget.Allocation.Height - 2);
                cr.Fill();
            }
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            if (this.player != null)
                this.player.Dispose();
            Bass.Free();
            Application.Quit();
        }

        private void OpenFileFromMenu(object sender, EventArgs a)
        {
            Gtk.FileChooserDialog dialog = new Gtk.FileChooserDialog("Open File", null, FileChooserAction.Open, "Open", Gtk.ResponseType.Accept, "Cancel", Gtk.ResponseType.Cancel);
            dialog.SelectMultiple = true;
            Gtk.FileFilter filter = new Gtk.FileFilter();
            filter.Name = "ogg File";
            filter.AddPattern("*.ogg");
            dialog.AddFilter(filter);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                AddSongs(dialog.Filenames);
            }
            dialog.Destroy();
        }

        private void AddSongs(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(paths[i]))
                {
                    try
                    {
                        using (var ii = new NVorbis.VorbisReader(paths[i]))
                        {
                            string title = !string.IsNullOrEmpty(ii.Tags.Title) ? ii.Tags.Title : System.IO.Path.GetFileName(paths[i]);
                            string time = ii.TotalTime.ToString();
                            string artist = !string.IsNullOrEmpty(ii.Tags.Artist) ? ii.Tags.Artist : "";
                            string loop = !string.IsNullOrEmpty(ii.Tags.GetTagSingle("LOOPSTART")) && (!string.IsNullOrEmpty(ii.Tags.GetTagSingle("LOOPLENGTH")) || !string.IsNullOrEmpty(ii.Tags.GetTagSingle("LOOPEND"))) ? "Loop" : "";
                            string path = paths[i];

                            _liststore.AppendValues(title, time, loop, artist, path);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }
            }
        }

        private void ShowAbout(object sender, EventArgs a)
        {
            var dia = new AboutDialog();
            dia.Icon = this.Icon;
            dia.Logo = this.Icon;
            dia.Documenters = new string[] { "Mr-Ojii" };
            dia.Authors = new string[] { "Mr-Ojii" };
            dia.LicenseType = License.MitX11;
            dia.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            dia.Title = "About LoopMusicPlayer";
            dia.ProgramName = "LoopMusicPlayer";
            dia.Comments = "MusicPlayer";
            dia.Copyright = "© 2021 Mr-Ojii";
            dia.Run();
            dia.Destroy();
        }
    }
}
