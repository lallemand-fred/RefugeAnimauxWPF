using System;
using System.Collections.Generic;
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
using RefugeAnimaux.classeMetier;
using RefugeAnimaux.coucheModeleVue;
using RefugeAnimaux.coucheVue;

namespace RefugeAnimaux
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Vue_Modele vueModele;

        public MainWindow()
        {
            InitializeComponent();

            // cree le vue-modele et le connecte a la fenetre
            vueModele = new Vue_Modele();
            this.DataContext = vueModele;
        }

        // active/desactive les boutons selon la selection
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool animalSelectionne = vueModele.AnimalSelectionne != null;
            btnModifier.IsEnabled = animalSelectionne;
            btnSupprimer.IsEnabled = animalSelectionne;
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            var formulaire = new FormulaireAnimal();
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.AjouterAnimal(formulaire.ResultAnimal);
                    MessageBox.Show("Animal ajoute !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            if (vueModele.AnimalSelectionne == null) return;

            var formulaire = new FormulaireAnimal(vueModele.AnimalSelectionne);
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.ModifierAnimal(formulaire.ResultAnimal);
                    vueModele.ChargerAnimaux();
                    MessageBox.Show("Animal modifie !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (vueModele.AnimalSelectionne == null) return;

            var result = MessageBox.Show(
                $"Supprimer l'animal '{vueModele.AnimalSelectionne.Nom}' ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    vueModele.SupprimerAnimal(vueModele.AnimalSelectionne);
                    MessageBox.Show("Animal supprime !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
