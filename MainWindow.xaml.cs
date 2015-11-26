using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Musique
{
  public partial class MainWindow : Window
    {
    // Global variables
    enum State { Device, Filter, Sound, Scheme, Notif };
    enum Sound { Fan, Fire, Forest, Rain, Waves, Wind };
    bool isOn;
    bool isPlaying;
    bool[] isFilt;
    int count;
    int currentRow;
    double currentVol;
    State st;
    Sound snd;
    Brush offColor;
    Brush onColor;
    Ellipse powerEll;
    Ellipse[] filterEll;
    Ellipse[] soundEll;
    Ellipse[] offColorEll;
    Ellipse[] onColorEll;
    Grid notifGrid;
    Label countLbl;
    MediaPlayer mp;
    Timer timcanpy;
    Uri currentSound;
    public MainWindow()
    {
      // Initialize global variables
      isOn = false;
      isPlaying = false;
      isFilt = new bool[7];
      count = 0;
      currentRow = 8;
      currentVol = 0.0;
      snd = Sound.Rain;
      st = State.Device;
      offColor = Brushes.Red;
      onColor = Brushes.Green;
      mp = new MediaPlayer();
      mp.MediaEnded += new EventHandler(loopAudio);
      currentSound = new Uri(Directory.GetCurrentDirectory() + "\\sounds\\rain.mp3");
      timcanpy = new Timer(5000);
      timcanpy.Elapsed += new ElapsedEventHandler(getNotif);
      timcanpy.AutoReset = true;
      timcanpy.Enabled = true;

      // Initialize GUI
      InitializeComponent();
      notifMenu();
      deviceMenu();

      // Connect menu labels to click events
      deviceLbl.MouseUp += new MouseButtonEventHandler(deviceLbl_MouseUp);
      filterLbl.MouseUp += new MouseButtonEventHandler(filterLbl_MouseUp);
      soundLbl.MouseUp += new MouseButtonEventHandler(soundLbl_MouseUp);
      colorLbl.MouseUp += new MouseButtonEventHandler(colorLbl_MouseUp);
      notifLbl.MouseUp += new MouseButtonEventHandler(notifLbl_MouseUp);

      // Connect menu labels to mouse hover
      deviceLbl.MouseEnter += new MouseEventHandler(enterLabel);
      filterLbl.MouseEnter += new MouseEventHandler(enterLabel);
      soundLbl.MouseEnter += new MouseEventHandler(enterLabel);
      colorLbl.MouseEnter += new MouseEventHandler(enterLabel);
      notifLbl.MouseEnter += new MouseEventHandler(enterLabel);
      deviceLbl.MouseLeave += new MouseEventHandler(leaveLabel);
      filterLbl.MouseLeave += new MouseEventHandler(leaveLabel);
      soundLbl.MouseLeave += new MouseEventHandler(leaveLabel);
      colorLbl.MouseLeave += new MouseEventHandler(leaveLabel);
      notifLbl.MouseLeave += new MouseEventHandler(leaveLabel);
    }

    // Mouse enter effect for labels
    private void enterLabel(object sender, MouseEventArgs e)
    {
      Label temp = sender as Label;

      if (onColor == Brushes.Black)
      {
        temp.Foreground = Brushes.DarkSlateGray;
      }
      else
      {
        temp.Foreground = onColor;
      }
    }

    // Mouse leave effect for labels
    private void leaveLabel(object sender, MouseEventArgs e)
    {
      Label temp = sender as Label;
      temp.Foreground = Brushes.Black;
    }

    // Mouse enter effect for ellipses
    private void enterEllipse(object sender, MouseEventArgs e)
    {
      Ellipse temp = sender as Ellipse;
      DropShadowEffect dse = new DropShadowEffect();
      dse.BlurRadius = 25.0;
      dse.Color = Colors.MidnightBlue;
      temp.Effect = dse;
    }

    // Mouse leave effect for ellipses
    private void leaveEllipse(object sender, MouseEventArgs e)
    {
      Ellipse temp = sender as Ellipse;
      temp.Effect = null;
    }

    // Load the data from Facebook
    private void getNotif(object sender, ElapsedEventArgs e)
    {
      // Only if the device is on
      if (isOn)
      {
        // Open the text file
        String s = Directory.GetCurrentDirectory() + "\\notif.txt";
        if (!File.Exists(s))
        {
          return;
        }
        StreamReader sr = File.OpenText(s);

        // Place data into array
        s = "";
        int[] notif = new int[7];
        int i = 0;
        while ((s = sr.ReadLine()) != null)
        {
          notif[i++] = int.Parse(s);
        }

        // Close the file
        sr.Close();

        // Filter data in array
        for (i = 0; i < 7; i++)
        {
          if (isFilt[i])
          {
            notif[i] = 0;
          }
        }

        // Update UI
        int sum = notif.Sum();
        if (sum > count)
        {
          sum = sum - count;
          for (i = 0; i < sum; i++)
          {
            addBlock();
          }
        }
        else if (sum < count)
        {
          sum = count - sum;
          for (i = 0; i < sum; i++)
          {
            removeBlock();
          }
        }
      }
    }

    // Clear the mainGrid of elements, columns and rows
    private void clearGrid()
    {
      mainGrid.Children.Clear();
      mainGrid.ColumnDefinitions.Clear();
      mainGrid.RowDefinitions.Clear();
    }

    // Convert an index to its corresponding color
    private Brush convertToColor(int i)
    {
      switch (i)
      {
        case 0:
          return Brushes.Black;
        case 1:
          return Brushes.Blue;
        case 2:
          return Brushes.Green;
        case 3:
          return Brushes.Orange;
        case 4:
          return Brushes.Pink;
        case 5:
          return Brushes.Purple;
        case 6:
          return Brushes.Red;
        case 7:
          return Brushes.White;
        default:
          return null;
      }
    }

    // Convert a sound to the correct URI
    private Uri convertSound(Sound s)
    {
      String str = "";

      switch (s)
      {
        case Sound.Fan:
          str = "\\sounds\\fan.mp3";
          break;
        case Sound.Fire:
          str = "\\sounds\\fire.mp3";
          break;
        case Sound.Forest:
          str = "\\sounds\\forest.mp3";
          break;
        case Sound.Rain:
          str = "\\sounds\\rain.mp3";
          break;
        case Sound.Waves:
          str = "\\sounds\\waves.mp3";
          break;
        case Sound.Wind:
          str = "\\sounds\\wind.mp3";
          break;
      }

      return new Uri(Directory.GetCurrentDirectory() + str);
    }

    // Add a block to the notification bar
    private void addBlock(bool d = false, int n = 0)
    {
      this.Dispatcher.Invoke((Action)(() =>
      {
        // No more than 10
        if (count <= 10)
        {
          // Only redrawing for menu
          if (d)
          {
            for (int i = 0; i < n; i++)
            {
              // Create a new block
              Rectangle block = new Rectangle();
              block.Stroke = Brushes.LightSeaGreen;
              block.Fill = Brushes.LightSeaGreen;
              block.Width = notifGrid.Width;
              block.Height = notifGrid.Height / 10.0;

              // Place block inside bar
              notifGrid.Children.Add(block);
              Grid.SetRow(block, 9 - i);
            }
          }

          else
          {
            // Update
            if (count == 10)
            {
              return;
            }
            count++;
            countLbl.Content = count.ToString();

            // Create a new block
            Rectangle block = new Rectangle();
            block.Stroke = Brushes.LightSeaGreen;
            block.Fill = Brushes.LightSeaGreen;
            block.Width = notifGrid.Width;
            block.Height = notifGrid.Height / 10.0;

            // Place block inside bar
            notifGrid.Children.Add(block);
            Grid.SetRow(block, count + currentRow);
            currentRow -= 2;
          }
        }

        if (isOn)
        {
          // Check the audio
          if (!isPlaying)
          {
            isPlaying = true;
            mp.Open(currentSound);
            mp.Play();
          }
          currentVol = count * 0.1;
          mp.Volume = currentVol;
        }
      }));
    }

    // Remove a block from the notification bar
    private void removeBlock()
    {
      this.Dispatcher.Invoke((Action)(() =>
      {
        // No less than 0
        if (count > 0)
        {
          // Update
          count--;
          currentRow += 2;
          countLbl.Content = count.ToString();

          // Remove the most recent block
          notifGrid.Children.RemoveAt(count);
        }

        if (isOn)
        {
          // Check the audio
          currentVol = count * 0.1;
          mp.Volume = currentVol;
          if (count == 0)
          {
            isPlaying = false;
            mp.Stop();
          }
        }
      }));
    }

    // Loop the audio
    private void loopAudio(object sender, EventArgs e)
    {
      mp.Position = TimeSpan.Zero;
      mp.Play();
    }

    // Create the Device menu
    private void deviceMenu()
    {
      // Clear the grid
      clearGrid();

      // Create 2 columns
      for (int i = 0; i < 2; i++)
      {
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      // Create and place label
      Label powerLbl = new Label();
      powerLbl.Content = "Power";
      powerLbl.FontFamily = new FontFamily("Trebuchet MS");
      powerLbl.FontSize = 36.0;
      powerLbl.Padding = new Thickness(20.0, 10.0, 0.0, 0.0);
      mainGrid.Children.Add(powerLbl);
      Grid.SetColumn(powerLbl, 0);

      // Create and place ellipses
      powerEll = new Ellipse();
      if (!isOn)
      {
        powerEll.Stroke = offColor;
        powerEll.Fill = offColor;
      }
      else
      {
        powerEll.Stroke = onColor;
        powerEll.Fill = onColor;
      }
      powerEll.Width = 50.0;
      powerEll.Height = 50.0;
      powerEll.HorizontalAlignment = HorizontalAlignment.Left;
      powerEll.VerticalAlignment = VerticalAlignment.Top;
      powerEll.Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
      powerEll.Cursor = Cursors.Hand;
      powerEll.MouseUp += new MouseButtonEventHandler(powerEll_MouseUp);
      powerEll.MouseEnter += new MouseEventHandler(enterEllipse);
      powerEll.MouseLeave += new MouseEventHandler(leaveEllipse);
      mainGrid.Children.Add(powerEll);
      Grid.SetColumn(powerEll, 1);
    }

    // Create the Filter menu
    private void filterMenu()
    {
      // Clear the grid
      clearGrid();

      // Create 2 columns
      for (int i = 0; i < 2; i++)
      {
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      // Create 7 rows
      for (int i = 0; i < 7; i++)
      {
        mainGrid.RowDefinitions.Add(new RowDefinition());
      }

      // Create and place the labels
      Label[] lbls = new Label[7];
      String[] cont = {"Likes", "Comments", "Tags", "Groups", "Birthdays", "Messages", "Friend Requests"};
      for (int i = 0; i < 7; i++)
      {
        lbls[i] = new Label();
        lbls[i].Content = cont[i];
        lbls[i].FontFamily = new FontFamily("Trebuchet MS");
        lbls[i].FontSize = 36.0;
        lbls[i].Padding = new Thickness(20.0, 10.0, 0.0, 0.0);
        mainGrid.Children.Add(lbls[i]);
        Grid.SetColumn(lbls[i], 0);
        Grid.SetRow(lbls[i], i);
      }

      // Create and place the ellipses
      filterEll = new Ellipse[7];
      for (int i = 0; i < 7; i++)
      {
        filterEll[i] = new Ellipse();
        if (!isFilt[i])
        {
          filterEll[i].Stroke = offColor;
          filterEll[i].Fill = offColor;
        }
        else
        {
          filterEll[i].Stroke = onColor;
          filterEll[i].Fill = onColor;
        }
        filterEll[i].Width = 50.0;
        filterEll[i].Height = 50.0;
        filterEll[i].HorizontalAlignment = HorizontalAlignment.Left;
        filterEll[i].VerticalAlignment = VerticalAlignment.Top;
        filterEll[i].Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
        filterEll[i].Cursor = Cursors.Hand;
        filterEll[i].MouseUp += new MouseButtonEventHandler(filterEll_MouseUp);
        filterEll[i].MouseEnter += new MouseEventHandler(enterEllipse);
        filterEll[i].MouseLeave += new MouseEventHandler(leaveEllipse);
        mainGrid.Children.Add(filterEll[i]);
        Grid.SetColumn(filterEll[i], 1);
        Grid.SetRow(filterEll[i], i);
      }
    }

    // Create the sound menu
    private void soundMenu()
    {
      // Clear the grid
      clearGrid();

      // Create 2 columns
      for (int i = 0; i < 2; i++)
      {
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      // Create 6 rows
      for (int i = 0; i < 6; i++)
      {
        mainGrid.RowDefinitions.Add(new RowDefinition());
      }

      // Create and place the labels
      Label[] lbls = new Label[6];
      String[] cont = { "Fan", "Fire", "Forest", "Rain", "Waves", "Wind" };
      for (int i = 0; i < 6; i++)
      {
        lbls[i] = new Label();
        lbls[i].Content = cont[i];
        lbls[i].FontFamily = new FontFamily("Trebuchet MS");
        lbls[i].FontSize = 36.0;
        lbls[i].Padding = new Thickness(20.0, 10.0, 0.0, 0.0);
        mainGrid.Children.Add(lbls[i]);
        Grid.SetColumn(lbls[i], 0);
        Grid.SetRow(lbls[i], i);
      }

      // Create and place the ellipses
      soundEll = new Ellipse[6];
      for (int i = 0; i < 6; i++)
      {
        soundEll[i] = new Ellipse();
        if (((int) snd) != i)
        {
          soundEll[i].Stroke = offColor;
          soundEll[i].Fill = offColor;
        }
        else
        {
          soundEll[i].Stroke = onColor;
          soundEll[i].Fill = onColor;
        }
        soundEll[i].Width = 50.0;
        soundEll[i].Height = 50.0;
        soundEll[i].HorizontalAlignment = HorizontalAlignment.Left;
        soundEll[i].VerticalAlignment = VerticalAlignment.Top;
        soundEll[i].Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
        soundEll[i].Cursor = Cursors.Hand;
        soundEll[i].MouseUp += new MouseButtonEventHandler(soundEll_MouseUp);
        soundEll[i].MouseEnter += new MouseEventHandler(enterEllipse);
        soundEll[i].MouseLeave += new MouseEventHandler(leaveEllipse);
        mainGrid.Children.Add(soundEll[i]);
        Grid.SetColumn(soundEll[i], 1);
        Grid.SetRow(soundEll[i], i);
      }
    }

    // Create the color menu
    private void colorMenu()
    {
      // Clear the grid
      clearGrid();

      // Create 3 columns
      for (int i = 0; i < 3; i++)
      {
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      // Create 9 rows
      for (int i = 0; i < 9; i++)
      {
        mainGrid.RowDefinitions.Add(new RowDefinition());
      }

      // Create and place the labels
      Label[] lbls = new Label[2];
      String[] cont = { "OFF", "ON" };
      for (int i = 0; i < 2; i++)
      {
        lbls[i] = new Label();
        lbls[i].Content = cont[i];
        lbls[i].FontFamily = new FontFamily("Trebuchet MS");
        lbls[i].FontSize = 56.0;
        lbls[i].Padding = new Thickness(0.0, 10.0, 0.0, 0.0);
        lbls[i].HorizontalAlignment = HorizontalAlignment.Center;
        mainGrid.Children.Add(lbls[i]);
        Grid.SetColumn(lbls[i], i + 1);
        Grid.SetRow(lbls[i], 0);
      }

      // Create and place the color labels
      lbls = new Label[8];
      cont = new String[] { "Black", "Blue", "Green", "Orange", "Pink", "Purple", "Red", "White" };
      for (int i = 0; i < 8; i++)
      {
        lbls[i] = new Label();
        lbls[i].Content = cont[i];
        lbls[i].FontFamily = new FontFamily("Trebuchet MS");
        lbls[i].FontSize = 36.0;
        lbls[i].Padding = new Thickness(20.0, 10.0, 0.0, 0.0);
        mainGrid.Children.Add(lbls[i]);
        Grid.SetColumn(lbls[i], 0);
        Grid.SetRow(lbls[i], i + 1);
      }

      // Create and place the off color ellipses
      offColorEll = new Ellipse[8];
      for (int i = 0; i < 8; i++)
      {
        offColorEll[i] = new Ellipse();
        if (offColor != convertToColor(i))
        {
          offColorEll[i].Stroke = offColor;
          offColorEll[i].Fill = offColor;
        }
        else
        {
          offColorEll[i].Stroke = onColor;
          offColorEll[i].Fill = onColor;
        }
        offColorEll[i].Width = 50.0;
        offColorEll[i].Height = 50.0;
        offColorEll[i].HorizontalAlignment = HorizontalAlignment.Center;
        offColorEll[i].VerticalAlignment = VerticalAlignment.Top;
        offColorEll[i].Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
        offColorEll[i].Cursor = Cursors.Hand;
        offColorEll[i].MouseUp += new MouseButtonEventHandler(offColorEll_MouseUp);
        offColorEll[i].MouseEnter += new MouseEventHandler(enterEllipse);
        offColorEll[i].MouseLeave += new MouseEventHandler(leaveEllipse);
        mainGrid.Children.Add(offColorEll[i]);
        Grid.SetColumn(offColorEll[i], 1);
        Grid.SetRow(offColorEll[i], i + 1);
      }

      // Create and place the on color ellipses
      onColorEll = new Ellipse[8];
      for (int i = 0; i < 8; i++)
      {
        onColorEll[i] = new Ellipse();
        if (onColor != convertToColor(i))
        {
          onColorEll[i].Stroke = offColor;
          onColorEll[i].Fill = offColor;
        }
        else
        {
          onColorEll[i].Stroke = onColor;
          onColorEll[i].Fill = onColor;
        }
        onColorEll[i].Width = 50.0;
        onColorEll[i].Height = 50.0;
        onColorEll[i].HorizontalAlignment = HorizontalAlignment.Center;
        onColorEll[i].VerticalAlignment = VerticalAlignment.Top;
        onColorEll[i].Margin = new Thickness(0.0, 10.0, 0.0, 0.0);
        onColorEll[i].Cursor = Cursors.Hand;
        onColorEll[i].MouseUp += new MouseButtonEventHandler(onColorEll_MouseUp);
        onColorEll[i].MouseEnter += new MouseEventHandler(enterEllipse);
        onColorEll[i].MouseLeave += new MouseEventHandler(leaveEllipse);
        mainGrid.Children.Add(onColorEll[i]);
        Grid.SetColumn(onColorEll[i], 2);
        Grid.SetRow(onColorEll[i], i + 1);
      }
    }

    // Create the notification menu
    private void notifMenu()
    {
      // Clear the grid
      clearGrid();

      // Create 3 rows
      for (int i = 0; i < 3; i++)
      {
        RowDefinition rd = new RowDefinition();

        if (i == 1)
        {
          rd.Height = new GridLength(4, GridUnitType.Star);
        }

        mainGrid.RowDefinitions.Add(rd);
      }

      // Create and place main label
      Label lbl = new Label();
      lbl.Content = "Notification Bar";
      lbl.FontFamily = new FontFamily("Trebuchet MS");
      lbl.FontSize = 36.0;
      lbl.Padding = new Thickness(0.0, 0.0, 0.0, 20.0);
      lbl.HorizontalAlignment = HorizontalAlignment.Center;
      lbl.VerticalAlignment = VerticalAlignment.Bottom;
      mainGrid.Children.Add(lbl);
      Grid.SetRow(lbl, 0);

      // Create border
      Border bord = new Border();
      bord.BorderBrush = Brushes.Black;
      bord.BorderThickness = new Thickness(2.0);
      bord.Width = 100.0;

      // Set and place notification grid
      notifGrid = new Grid();
      for (int i = 0; i < 10; i++)
      {
        notifGrid.RowDefinitions.Add(new RowDefinition());
      }
      addBlock(true, count);
      bord.Child = notifGrid;

      // Place border
      mainGrid.Children.Add(bord);
      Grid.SetRow(bord, 1);

      // Place the count label
      countLbl = new Label();
      countLbl.Content = count.ToString();
      countLbl.FontFamily = new FontFamily("Trebuchet MS");
      countLbl.FontSize = 36.0;
      countLbl.Padding = new Thickness(0.0, 20.0, 0.0, 0.0);
      countLbl.HorizontalAlignment = HorizontalAlignment.Center;
      mainGrid.Children.Add(countLbl);
      Grid.SetRow(countLbl, 2);
    }

    // Device label clicked
    private void deviceLbl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (st != State.Device)
      {
        st = State.Device;
        deviceMenu();

        if (isOn)
        {
          helpMsg.Text = this.FindResource("Customize") as String;
        }
      }
    }

    // Filter label clicked
    private void filterLbl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (st != State.Filter)
      {
        st = State.Filter;
        filterMenu();

        if (isOn)
        {
          helpMsg.Text = this.FindResource("Filter") as String;
        }
      }
    }

    // Sound label clicked
    private void soundLbl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (st != State.Sound)
      {
        st = State.Sound;
        soundMenu();

        if (isOn)
        {
          helpMsg.Text = this.FindResource("Sound") as String;
        }
      }
    }

    // Color label clicked
    private void colorLbl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (st != State.Scheme)
      {
        st = State.Scheme;
        colorMenu();

        if (isOn)
        {
          helpMsg.Text = this.FindResource("Color") as String;
        }
      }
    }

    // Notification label clicked
    private void notifLbl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (st != State.Notif)
      {
        st = State.Notif;
        notifMenu();

        if (isOn)
        {
          helpMsg.Text = this.FindResource("Notif") as String;
        }
      }
    }

    // Power ellipse clicked
    private void powerEll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Off -> On
      if (!isOn)
      {
        isOn = true;
        powerEll.Stroke = onColor;
        powerEll.Fill = onColor;
        notifMenu();
        deviceMenu();
        helpMsg.Text = this.FindResource("Customize") as String;
      }

      // On -> Off
      else
      {
        isOn = false;
        powerEll.Stroke = offColor;
        powerEll.Fill = offColor;
        mp.Stop();
        isPlaying = false;
        helpMsg.Text = this.FindResource("Welcome") as String;
      }
    }

    // Filter ellipse clicked
    private void filterEll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Only if the device is on
      if (isOn)
      {
        // Find the index associated with the ellipse
        int idx = Array.IndexOf(filterEll, (Ellipse) sender);

        // Off -> On
        if (!isFilt[idx])
        {
          isFilt[idx] = true;
          filterEll[idx].Stroke = onColor;
          filterEll[idx].Fill = onColor;
        }

        // On -> Off
        else
        {
          isFilt[idx] = false;
          filterEll[idx].Stroke = offColor;
          filterEll[idx].Fill = offColor;
        }
      }
    }

    // Sound ellipse clicked
    private void soundEll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Only if the device is on
      if (isOn)
      {
        // Find the index associated with the ellipse
        int idx = Array.IndexOf(soundEll, (Ellipse) sender);

        // Turn current sound off
        soundEll[(int) snd].Stroke = offColor;
        soundEll[(int) snd].Fill = offColor;

        // Update the current sound
        snd = (Sound) idx;
        currentSound = convertSound(snd);
        if (isPlaying)
        {
          mp.Stop();
          mp.Open(currentSound);
          mp.Play();
        }

        // Turn current sound on
        soundEll[idx].Stroke = onColor;
        soundEll[idx].Fill = onColor;
      }
    }

    // Off color ellipse clicked
    private void offColorEll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Only if the device is on
      if (isOn)
      {
        // Find the index associated with the ellipse
        int idx = Array.IndexOf(offColorEll, (Ellipse) sender);

        // Ensure off and on aren't same color
        if (convertToColor(idx) != onColor)
        {
          // Update the off color
          offColor = convertToColor(idx);
        }

        // Force a redraw
        colorMenu();
      }
    }

    // On color ellipse clicked
    private void onColorEll_MouseUp(object sender, MouseButtonEventArgs e)
    {
      // Only if the device is on
      if (isOn)
      {
        // Find the index associated with the ellipse
        int idx = Array.IndexOf(onColorEll, (Ellipse) sender);

        // Ensure off and on aren't same color
        if (convertToColor(idx) != offColor)
        {
          // Update the on color
          onColor = convertToColor(idx);
        }

        // Force a redraw
        colorMenu();
      }
    }
  }
}
