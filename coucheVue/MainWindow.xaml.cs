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

            // charge les adoptions au demarrage
            ChargerAdoptions();
        }

        // active/desactive les boutons selon la selection
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool animalSelectionne = vueModele.AnimalSelectionne != null;
            btnModifier.IsEnabled = animalSelectionne;
            btnSupprimer.IsEnabled = animalSelectionne;
        }

        private void BtnArrivee_Click(object sender, RoutedEventArgs e)
        {
            var formulaire = new FormulaireArrivee(vueModele.ListeAnimaux, vueModele.ListeContacts, vueModele.ObtenirListeAdoptions());
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.EnregistrerArrivee(
                        formulaire.ResultAnimal,
                        formulaire.EstNouvelAnimal,
                        formulaire.ResultEntree,
                        formulaire.NouveauContact
                    );
                    MessageBox.Show("Arrivee enregistree !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSortie_Click(object sender, RoutedEventArgs e)
        {
            // on passe les animaux presents + contacts + adoptions pour le filtrage
            var formulaire = new FormulaireSortie(vueModele.GetAnimauxPresents(), vueModele.ListeContacts, vueModele.ObtenirListeAdoptions());
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.EnregistrerSortie(formulaire.ResultSortie);
                    MessageBox.Show("Sortie enregistree !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'enregistrement de la sortie: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // ============================================================
        // HANDLERS CONTACTS
        // ============================================================

        // active/desactive les boutons contacts selon la selection
        private void DataGridContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool contactSelectionne = vueModele.ContactSelectionne != null;
            btnModifierContact.IsEnabled = contactSelectionne;
            btnSupprimerContact.IsEnabled = contactSelectionne;
        }

        private void BtnAjouterContact_Click(object sender, RoutedEventArgs e)
        {
            var formulaire = new FormulaireContact();
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.AjouterContact(formulaire.ResultContact);
                    MessageBox.Show("Contact ajoute !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnModifierContact_Click(object sender, RoutedEventArgs e)
        {
            if (vueModele.ContactSelectionne == null) return;

            var formulaire = new FormulaireContact(vueModele.ContactSelectionne);
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.ModifierContact(formulaire.ResultContact);
                    vueModele.ChargerContacts();
                    MessageBox.Show("Contact modifie !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSupprimerContact_Click(object sender, RoutedEventArgs e)
        {
            if (vueModele.ContactSelectionne == null) return;

            var result = MessageBox.Show(
                $"Supprimer le contact '{vueModele.ContactSelectionne.Nom} {vueModele.ContactSelectionne.Prenom}' ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    vueModele.SupprimerContact(vueModele.ContactSelectionne);
                    MessageBox.Show("Contact supprime !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ============================================================
        // HANDLERS ADOPTIONS
        // ============================================================

        // adoption selectionnee dans le datagrid
        private Adoption adoptionSelectionnee;

        private void DataGridAdoptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            adoptionSelectionnee = dgAdoptions.SelectedItem as Adoption;
            btnModifierAdoption.IsEnabled = adoptionSelectionnee != null;
            btnSupprimerAdoption.IsEnabled = adoptionSelectionnee != null;
        }

        private void BtnNouvelleAdoption_Click(object sender, RoutedEventArgs e)
        {
            var formulaire = new FormulaireAdoption(vueModele.ListeAnimaux, vueModele.ListeContacts);
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.AjouterAdoption(formulaire.ResultAdoption);
                    MessageBox.Show("Demande d'adoption enregistree !");
                    ChargerAdoptions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnModifierAdoption_Click(object sender, RoutedEventArgs e)
        {
            if (adoptionSelectionnee == null) return;

            var formulaire = new FormulaireAdoption(adoptionSelectionnee, vueModele.ListeAnimaux, vueModele.ListeContacts);
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                try
                {
                    vueModele.ModifierStatutAdoption(formulaire.ResultAdoption);
                    MessageBox.Show("Statut modifie !");
                    ChargerAdoptions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSupprimerAdoption_Click(object sender, RoutedEventArgs e)
        {
            if (adoptionSelectionnee == null) return;

            var result = MessageBox.Show(
                $"Supprimer la demande d'adoption pour l'animal '{adoptionSelectionnee.AnimalId}' ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    vueModele.SupprimerAdoption(adoptionSelectionnee);
                    MessageBox.Show("Demande d'adoption supprimee !");
                    ChargerAdoptions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRafraichirAdoptions_Click(object sender, RoutedEventArgs e)
        {
            ChargerAdoptions();
        }

        private void ChargerAdoptions()
        {
            try
            {
                // nettoie les adoptions orphelines avant de charger
                int supprimees = vueModele.NettoyerAdoptionsOrphelines();
                if (supprimees > 0)
                {
                    MessageBox.Show($"{supprimees} adoption(s) orpheline(s) supprimee(s) (animal ou contact inexistant).",
                        "Nettoyage", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                dgAdoptions.ItemsSource = vueModele.ObtenirListeAdoptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement adoptions: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
