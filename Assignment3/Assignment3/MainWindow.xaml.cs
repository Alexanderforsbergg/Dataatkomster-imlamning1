using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeographyTools;
using Microsoft.EntityFrameworkCore;
using Windows.Devices.Geolocation;

namespace Assignment3
{
    public class AppDbContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Movies> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cinema>()
                .HasIndex(p => new { p.Name })
                .IsUnique(true);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DataAccessGUIAssignment;Integrated Security=True");
        }
    }
    public class Ticket
    {
        [Key]
        [Required]
        //[DisplayName("ID")]
        [Column(TypeName = "int")]
        public int ID { get; set; }

        [Required]
        //[DisplayName("ScreeningsID")]
        [Column(TypeName = "int")]
        public int ScreeningID { get; set; }

        [Required]
        public Screening Screening { get; set; }

        [Required]
        //[DisplayName("TimePurchased")]
        [Column(TypeName = "datetime")]
        public DateTime TimePurchased { get; set; }
    }

    public class Cinema
    {
        [Key]
        [Required(ErrorMessage = "This field is required")]
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(255, ErrorMessage = "Maximun lenth 255")]
        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(255, ErrorMessage = "Maximun lenth 255")]
        [Column(TypeName = "nvarchar(255)")]
        public string City { get; set; }
    }

    public class Screening
    {
        [Key]
        [Required(ErrorMessage = "This field is required")]
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Column(TypeName = "time(0)")]
        public TimeSpan Time { get; set; }  // ska vara time(0)

        [Column(TypeName = "int")]
        [Required(ErrorMessage = "This field is required")]
        public int MovieID { get; set; }

        [Column(TypeName = "int")]
        [Required(ErrorMessage = "This field is required")]
        public int CinemaID { get; set; }
        public Movies Movie { get; set; }
        public Cinema Cinema { get; set; }
    }



    public class Movies
    {
        [Key]
        [Column(TypeName = "int")]
        [Required(ErrorMessage = "This field is required")]
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(255, ErrorMessage = "Maximun lenth 255")]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Column(TypeName = "smallint")]
        [Required(ErrorMessage = "This field is required")]
        public Int16 Runtime { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Column(TypeName = "date")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(255, ErrorMessage = "Maximun lenth 255")]
        [Column(TypeName = "nvarchar(255)")]
        public string PosterPath { get; set; }
    }
    public partial class MainWindow : Window
    {
        private Thickness spacing = new Thickness(5);
        private FontFamily mainFont = new FontFamily("Constantia");

        // Some GUI elements that we need to access in multiple methods.
        private ComboBox cityComboBox;
        private ListBox cinemaListBox;
        private StackPanel screeningPanel;
        private StackPanel ticketPanel;

        private AppDbContext database = new AppDbContext();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            //options.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=DataAccessGUIAssignment;Integrated Security=True");
            //connection.Open();

            // Window options
            Title = "Cinemania";
            Width = 1000;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brushes.Black;

            // Main grid
            var grid = new Grid();
            Content = grid;
            grid.Margin = spacing;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            AddToGrid(grid, CreateCinemaGUI(), 0, 0);
            AddToGrid(grid, CreateScreeningGUI(), 0, 1);
            AddToGrid(grid, CreateTicketGUI(), 0, 2);
        }

        // Create the cinema part of the GUI: the left column.
        private UIElement CreateCinemaGUI()
        {
            var grid = new Grid
            {
                MinWidth = 200
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "Select Cinema",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            // Create the dropdown of cities.
            cityComboBox = new ComboBox
            {
                Margin = spacing
            };
            foreach (string city in GetCities())
            {
               cityComboBox.Items.Add(city);
            }
            cityComboBox.SelectedIndex = 0;
            AddToGrid(grid, cityComboBox, 1, 0);

            // When we select a city, update the GUI with the cinemas in the currently selected city.
            cityComboBox.SelectionChanged += (sender, e) =>
            {
                UpdateCinemaList();
            };

            // Create the dropdown of cinemas.
            cinemaListBox = new ListBox
            {
                Margin = spacing
            };
            AddToGrid(grid, cinemaListBox, 2, 0);
            UpdateCinemaList();

            // When we select a cinema, update the GUI with the screenings in the currently selected cinema.
            cinemaListBox.SelectionChanged += (sender, e) =>
            {
                UpdateScreeningList();
            };

            return grid;
        }

        // Create the screening part of the GUI: the middle column.
        private UIElement CreateScreeningGUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "Select Screening",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            var scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddToGrid(grid, scroll, 1, 0);

            screeningPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            scroll.Content = screeningPanel;

            UpdateScreeningList();

            return grid;
        }

        // Create the ticket part of the GUI: the right column.
        private UIElement CreateTicketGUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "My Tickets",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            var scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddToGrid(grid, scroll, 1, 0);

            ticketPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            scroll.Content = ticketPanel;

            // Update the GUI with the initial list of tickets.
            UpdateTicketList();
            return grid;
        }

        // Get a list of all cities that have cinemas in them.
        private IEnumerable<string> GetCities()
        {
            List<string> stad2 = new List<string>();
            var cityName = database.Cinemas.Select(x => new { x.City }).Distinct();
           
            foreach (var v in cityName)
            {
                cityComboBox.Items.Add(v.City);
            }
            return stad2;
        }

        // Get a list of all cinemas in the currently selected city.
        private IEnumerable<string> GetCinemasInSelectedCity()
        {
            List<string> cinemas = new List<string>();

            var cityChoice = (string)cityComboBox.SelectedItem;

            var cinemaCity = database.Cinemas.Where(a => a.City == cityChoice);

            foreach (var c in cinemaCity)
            {
                cinemas.Add(c.Name);
            }

            return cinemas;
        }

        // Update the GUI with the cinemas in the currently selected city.
        private void UpdateCinemaList()
        {
            cinemaListBox.Items.Clear();
            foreach (string cinema in GetCinemasInSelectedCity())
            {
                cinemaListBox.Items.Add(cinema);
            }
        }

        // Update the GUI with the screenings in the currently selected cinema.
        private void UpdateScreeningList()
        {
            screeningPanel.Children.Clear();

            var cinemaChoice = (string)cinemaListBox.SelectedItem;
            var cinemaNames = database.Screenings.Include(m=>m.Movie).Where(a => a.Cinema.Name == cinemaChoice);

            foreach (var item in cinemaNames)
            {
                // Create the button that will show all the info about the screening and let us buy a ticket for it.
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = spacing,
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };
                screeningPanel.Children.Add(button);
                int screeningID = item.ID;

                // When we click a screening, buy a ticket for it and update the GUI with the latest list of tickets.
                button.Click += (sender, e) =>
                {
                    BuyTicket(screeningID);
                };

                // The rest of this method is just creating the GUI element for the screening.
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                button.Content = grid;

                var image = CreateImage(@"Posters\" + item.Movie.PosterPath);
                image.Width = 50;
                image.Margin = spacing;
                image.ToolTip = new ToolTip { Content = item.Movie.Title };
                AddToGrid(grid, image, 0, 0);
                Grid.SetRowSpan(image, 3);

                var time = item.Time;
                var timeString = TimeSpanToString(time);
                var timeHeading = new TextBlock
                {
                    Text = timeString,
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Yellow
                };
                AddToGrid(grid, timeHeading, 0, 1);

                var titleHeading = new TextBlock
                {
                    Text = item.Movie.Title,
                    Margin = spacing,
                    FontFamily = mainFont,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                AddToGrid(grid, titleHeading, 1, 1);

                var releaseDate = item.Movie.ReleaseDate.Year;
                int runtimeMinutes = item.Movie.Runtime;
                var runtime = TimeSpan.FromMinutes(runtimeMinutes);
                string runtimeString = runtime.Hours + "h " + runtime.Minutes + "m";
                var details = new TextBlock
                {
                    Text = "📆 " + releaseDate + "     ⏳ " + runtimeString,
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    Foreground = Brushes.Silver
                };
                AddToGrid(grid, details, 2, 1);
            }
        }
        // Buy a ticket for the specified screening and update the GUI with the latest list of tickets.
        private void BuyTicket(int screeningID)
        {
            // First check if we already have a ticket for this screening.
            var chosenScreening = database.Tickets.Include(s=>s.Screening).Where(s => s.ScreeningID == screeningID);
            // If we don't, add it.
            if (chosenScreening.Count() == 0)
            {
                Ticket ticket = new Ticket();
                ticket.ScreeningID = screeningID;
                ticket.TimePurchased = DateTime.Now;

                database.Add(ticket);
                database.SaveChanges();

                UpdateTicketList();
            }
            else
            {
                MessageBox.Show("Already got this ticket");
            }
        }

        // Update the GUI with the latest list of tickets
        private void UpdateTicketList()
        {
            ticketPanel.Children.Clear();

            var movieTickets = database.Tickets.Include(s => s.Screening.Movie);

                // For each ticket:
                foreach (var t in movieTickets.OrderByDescending(i => i.TimePurchased))
                {
                // Create the button that will show all the info about the ticket and let us remove it.
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = spacing,
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };
                ticketPanel.Children.Add(button);
                int ticketID = Convert.ToInt32(t.ID);

                // When we click a ticket, remove it and update the GUI with the latest list of tickets.
                button.Click += (sender, e) =>
                {
                    RemoveTicket(ticketID);
                };

                // The rest of this method is just creating the GUI element for the screening.
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                button.Content = grid;

                var image = CreateImage(@"Posters\" + t.Screening.Movie.PosterPath);
                image.Width = 30;
                image.Margin = spacing;
                image.ToolTip = new ToolTip { Content = t.Screening.Movie.Title };
                AddToGrid(grid, image, 0, 0);
                Grid.SetRowSpan(image, 2);

                var titleHeading = new TextBlock
                {
                    Text = Convert.ToString(t.Screening.Movie.Title),
                    Margin = spacing,
                    FontFamily = mainFont,
                    FontSize = 14,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                AddToGrid(grid, titleHeading, 0, 1);

                var time = t.Screening.Time;
                var cinema = t.Screening.Cinema.Name;
                string timeString = TimeSpanToString(time);
                var timeAndCinemaHeading = new TextBlock
                {
                    Text = timeString + " - " + cinema,
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Yellow,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                AddToGrid(grid, timeAndCinemaHeading, 1, 1);
            }
        }

        // Remove the ticket for the specified screening and update the GUI with the latest list of tickets.
        private void RemoveTicket(int ticketID)
        {
                var ticket = database.Tickets.Where(x => x.ID == ticketID).FirstOrDefault();

                database.Tickets.Remove(ticket);
                database.SaveChanges();

                UpdateTicketList();
        }

        // Helper method to add a GUI element to the specified row and column in a grid.
        private void AddToGrid(Grid grid, UIElement element, int row, int column)
        {
            grid.Children.Add(element);
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
        }

        // Helper method to create a high-quality image for the GUI.
        private Image CreateImage(string filePath)
        {
            ImageSource source = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
            Image image = new Image
            {
                Source = source,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            return image;
        }

        // Helper method to turn a TimeSpan object into a string, such as 2:05.
        private string TimeSpanToString(TimeSpan timeSpan)
        {
            string hourString = timeSpan.Hours.ToString().PadLeft(2, '0');
            string minuteString = timeSpan.Minutes.ToString().PadLeft(2, '0');
            string timeString = hourString + ":" + minuteString;
            return timeString;
        }
    }
}
