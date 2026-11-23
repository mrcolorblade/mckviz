using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace KvizJatek
{
    public partial class MainWindow : Window
    {
        private readonly AdatbazisKezelo _db = new();
        private List<Kerdes> _aktualisKerdesek = new();
        private int _aktualisIndex = 0;
        private int _helyesValaszok = 0;
        private bool _jatekFut = false;
        private readonly MediaPlayer _zeneLejatszo = new();
        private bool _zeneSzol = true;

        public MainWindow()
        {
            InitializeComponent();
            HatterzeneInditasa();
            AdatokFrissitese();
            UjJatekInditasa();
        }

        private void AdatokFrissitese()
        {
            try
            {
                dgKerdesek.ItemsSource = _db.KerdesekLekerese();
                dgEredmenyek.ItemsSource = _db.EredmenyekLekerese();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Adatbázis kapcsolódási hiba:\n{ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //JÁTÉK LOGIKA

        private void BtnUjJatek_Click(object sender, RoutedEventArgs e)
        {
            UjJatekInditasa();
        }

        private void UjJatekInditasa()
        {
            try
            {
                _aktualisKerdesek = _db.KerdesekLekerese(keverve: true);
            }
            catch
            {
                return;
            }

            if (_aktualisKerdesek.Count == 0)
            {
                txtKerdes.Text = "Nincs kérdés az adatbázisban! Adj hozzá a 'Kérdések kezelése' fülön.";
                imgKerdes.Visibility = Visibility.Collapsed;
                txtNincsKep.Text = "Nincs kép";
                txtNincsKep.Visibility = Visibility.Visible;
                ValaszGombokAllapota(false);
                return;
            }

            _aktualisIndex = 0;
            _helyesValaszok = 0;
            _jatekFut = true;
            KovetkezoKerdesMegjelenitese();
        }

        private void KovetkezoKerdesMegjelenitese()
        {
            txtPontszam.Text = $"Kérdés: {_aktualisIndex + 1}/{_aktualisKerdesek.Count} | Helyes: {_helyesValaszok}";
            var kerdes = _aktualisKerdesek[_aktualisIndex];

            txtKerdes.Text = kerdes.KerdesSzoveg;
            btnA.Content = $"A: {kerdes.ValaszA}";
            btnB.Content = $"B: {kerdes.ValaszB}";
            btnC.Content = $"C: {kerdes.ValaszC}";
            btnD.Content = $"D: {kerdes.ValaszD}";

            KepBetoltese(kerdes.KepNev);
            ValaszGombokAllapota(true);
        }

        private void KepBetoltese(string? kepNev)
        {
            if (string.IsNullOrWhiteSpace(kepNev))
            {
                imgKerdes.Source = null;
                imgKerdes.Visibility = Visibility.Collapsed;
                txtNincsKep.Text = "Nincs kép ehhez a kérdéshez";
                txtNincsKep.Visibility = Visibility.Visible;
                return;
            }

            string utvonal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Kepek", kepNev);
            if (File.Exists(utvonal))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(utvonal, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgKerdes.Source = bitmap;
                imgKerdes.Visibility = Visibility.Visible;
                txtNincsKep.Visibility = Visibility.Collapsed;
            }
            else
            {
                imgKerdes.Source = null;
                imgKerdes.Visibility = Visibility.Collapsed;
                txtNincsKep.Text = $"A kép nem található:\nKepek\\{kepNev}";
                txtNincsKep.Visibility = Visibility.Visible;
            }
        }

        private void ValaszGomb_Click(object sender, RoutedEventArgs e)
        {
            if (!_jatekFut || sender is not Button gomb) return;

            string valasztott = gomb.Tag?.ToString() ?? "";
            var aktualisKerdes = _aktualisKerdesek[_aktualisIndex];

            if (valasztott == aktualisKerdes.HelyesValasz)
            {
                _helyesValaszok++;
            }

            _aktualisIndex++;

            if (_aktualisIndex < _aktualisKerdesek.Count)
            {
                KovetkezoKerdesMegjelenitese();
            }
            else
            {
                JatekVege();
            }
        }

        private void JatekVege()
        {
            _jatekFut = false;
            ValaszGombokAllapota(false);

            int osszes = _aktualisKerdesek.Count;
            int hibas = osszes - _helyesValaszok;
            double szazalek = osszes > 0 ? Math.Round((double)_helyesValaszok / osszes * 100, 1) : 0;

            txtPontszam.Text = $"Végeredmény: {_helyesValaszok}/{osszes} ({szazalek}%)";

            string statisztika =
                $"Vége a kvíznek! Íme a statisztikád:\n\n" +
                $"• Összes kérdés: {osszes}\n" +
                $"• Helyes válaszok: {_helyesValaszok}\n" +
                $"• Hibás válaszok: {hibas}\n" +
                $"• Eredményesség: {szazalek}%\n\n" +
                $"Az eredményt dátummal együtt elmentettük az adatbázisba.";

            try
            {
                _db.EredmenyMentese(txtJatekosNev.Text, _helyesValaszok);
                dgEredmenyek.ItemsSource = _db.EredmenyekLekerese();
                MessageBox.Show(statisztika, "Kvíz statisztika", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nem sikerült menteni az eredményt:\n{ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValaszGombokAllapota(bool engedelyezve)
        {
            btnA.IsEnabled = engedelyezve;
            btnB.IsEnabled = engedelyezve;
            btnC.IsEnabled = engedelyezve;
            btnD.IsEnabled = engedelyezve;
        }

        //KÉRDÉSEK
        private void DgKerdesek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKerdesek.SelectedItem is not Kerdes k) return;

            txtKerdesSzoveg.Text = k.KerdesSzoveg;
            txtValaszA.Text = k.ValaszA;
            txtValaszB.Text = k.ValaszB;
            txtValaszC.Text = k.ValaszC;
            txtValaszD.Text = k.ValaszD;
            txtKepNev.Text = k.KepNev ?? string.Empty;
            cmbHelyesValasz.SelectedIndex = k.HelyesValasz switch
            {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "D" => 3,
                _ => 0
            };
        }

        private void BtnHozzaadas_Click(object sender, RoutedEventArgs e)
        {
            if (!MezokEllenorzese()) return;

            var ujKerdes = UrlapbolKerdes();
            _db.KerdesHozzaadasa(ujKerdes);
            AdatokFrissitese();
            MezokUritese();
        }

        private void BtnModositas_Click(object sender, RoutedEventArgs e)
        {
            if (dgKerdesek.SelectedItem is not Kerdes kivalasztott)
            {
                MessageBox.Show("Előbb válassz ki egy kérdést a táblázatból!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!MezokEllenorzese()) return;

            var modositott = UrlapbolKerdes();
            modositott.Id = kivalasztott.Id;

            _db.KerdesModositasa(modositott);
            AdatokFrissitese();
            MezokUritese();
        }

        private void BtnTorles_Click(object sender, RoutedEventArgs e)
        {
            if (dgKerdesek.SelectedItem is not Kerdes kivalasztott)
            {
                MessageBox.Show("Előbb válassz ki egy kérdést a táblázatból!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var valasz = MessageBox.Show($"Biztosan törlöd ezt a kérdést?\n\n\"{kivalasztott.KerdesSzoveg}\"",
                                         "Törlés megerősítése", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (valasz == MessageBoxResult.Yes)
            {
                _db.KerdesTorlese(kivalasztott.Id);
                AdatokFrissitese();
                MezokUritese();
            }
        }

        private void BtnMezokUritese_Click(object sender, RoutedEventArgs e)
        {
            MezokUritese();
        }

        private bool MezokEllenorzese()
        {
            if (string.IsNullOrWhiteSpace(txtKerdesSzoveg.Text) ||
                string.IsNullOrWhiteSpace(txtValaszA.Text) ||
                string.IsNullOrWhiteSpace(txtValaszB.Text) ||
                string.IsNullOrWhiteSpace(txtValaszC.Text) ||
                string.IsNullOrWhiteSpace(txtValaszD.Text))
            {
                MessageBox.Show("A kérdés és a 4 válasz mezőt kötelező kitölteni!", "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private Kerdes UrlapbolKerdes()
        {
            string helyes = (cmbHelyesValasz.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "A";
            return new Kerdes
            {
                KerdesSzoveg = txtKerdesSzoveg.Text.Trim(),
                ValaszA = txtValaszA.Text.Trim(),
                ValaszB = txtValaszB.Text.Trim(),
                ValaszC = txtValaszC.Text.Trim(),
                ValaszD = txtValaszD.Text.Trim(),
                HelyesValasz = helyes,
                KepNev = string.IsNullOrWhiteSpace(txtKepNev.Text) ? null : txtKepNev.Text.Trim()
            };
        }

        private void MezokUritese()
        {
            txtKerdesSzoveg.Clear();
            txtValaszA.Clear();
            txtValaszB.Clear();
            txtValaszC.Clear();
            txtValaszD.Clear();
            txtKepNev.Clear();
            cmbHelyesValasz.SelectedIndex = 0;
            dgKerdesek.SelectedItem = null;
        }

        //EREDMÉNYEK TÖRLÉSE

        private void BtnEredmenyTorles_Click(object sender, RoutedEventArgs e)
        {
            if (dgEredmenyek.SelectedItem is not Eredmeny kivalasztott)
            {
                MessageBox.Show("Válassz ki egy eredményt a törléshez!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _db.EredmenyTorlese(kivalasztott.Id);
            AdatokFrissitese();
        }

        //Zene
        private void HatterzeneInditasa()
        {
            string zeneUtvonal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Zene", "zene.mp3");
            if (!File.Exists(zeneUtvonal)) return;

            _zeneLejatszo.Open(new Uri(zeneUtvonal, UriKind.Absolute));
            _zeneLejatszo.Volume = 0.3; 

            _zeneLejatszo.MediaEnded += (s, e) =>
            {
                _zeneLejatszo.Position = TimeSpan.Zero;
                _zeneLejatszo.Play();
            };

            _zeneLejatszo.Play();
        }

        private void BtnZene_Click(object sender, RoutedEventArgs e)
        {
            if (_zeneSzol)
            {
                _zeneLejatszo.Pause();
                btnZene.Content = "Zene: KI";
                _zeneSzol = false;
            }
            else
            {
                _zeneLejatszo.Play();
                btnZene.Content = "Zene: BE";
                _zeneSzol = true;
            }
        }
    }
}