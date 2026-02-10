using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RefugeAnimaux.classeMetier;
using RefugeAnimaux.coucheModeleVue;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireVaccination : Window
    {
        private Animal animal;
        private Vue_Modele vueModele;

        // mode modification : garde l'ancienne vaccination pour la supprimer
        private bool enModeModification = false;
        private Vaccination vaccinationOriginale = null;

        public FormulaireVaccination(Animal animal, Vue_Modele vueModele)
        {
            InitializeComponent();

            this.animal = animal;
            this.vueModele = vueModele;

            // affiche les infos de l'animal
            this.Title = "Vaccinations de " + animal.Nom;
            txtNom.Text = animal.Nom;
            txtType.Text = animal.Type;
            txtIdentifiant.Text = animal.Identifiant;

            // date par defaut = aujourd'hui
            dpDateVaccin.SelectedDate = DateTime.Today;

            // charge les vaccins dispo dans le combo
            ChargerVaccins();

            // charge les vaccinations existantes
            ChargerVaccinations();
        }

        // remplit le ComboBox avec les vaccins de la table VACCIN
        private void ChargerVaccins()
        {
            try
            {
                List<string> vaccins = vueModele.ObtenirListeVaccins();
                cmbVaccin.Items.Clear();
                foreach (string vaccin in vaccins)
                {
                    cmbVaccin.Items.Add(vaccin);
                }
                if (cmbVaccin.Items.Count > 0)
                    cmbVaccin.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement vaccins: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // charge les vaccinations de l'animal dans le DataGrid
        private void ChargerVaccinations()
        {
            try
            {
                List<Vaccination> vaccinations = vueModele.ObtenirVaccinationsAnimal(animal.Identifiant);
                dgVaccinations.ItemsSource = vaccinations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement vaccinations: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // active/desactive le bouton Modifier selon la selection
        private void DgVaccinations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnModifier.IsEnabled = dgVaccinations.SelectedItem != null;
        }

        // passe en mode modification : remplit le formulaire avec la vaccination selectionnee
        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            Vaccination vac = dgVaccinations.SelectedItem as Vaccination;
            if (vac == null) return;

            enModeModification = true;
            vaccinationOriginale = vac;

            // remplit les champs avec les valeurs actuelles
            for (int i = 0; i < cmbVaccin.Items.Count; i++)
            {
                if (cmbVaccin.Items[i].ToString() == vac.Vaccin)
                {
                    cmbVaccin.SelectedIndex = i;
                    break;
                }
            }
            dpDateVaccin.SelectedDate = vac.Date;

            // change le texte du bouton et du groupbox
            btnAjouter.Content = "Enregistrer";
            grpFormulaire.Header = "Modifier la vaccination";
            btnAnnulerModif.Visibility = Visibility.Visible;
        }

        // annule le mode modification et remet le formulaire en mode ajout
        private void ResetModeAjout()
        {
            enModeModification = false;
            vaccinationOriginale = null;
            btnAjouter.Content = "Ajouter";
            grpFormulaire.Header = "Ajouter une vaccination";
            btnAnnulerModif.Visibility = Visibility.Collapsed;
            dpDateVaccin.SelectedDate = DateTime.Today;
            if (cmbVaccin.Items.Count > 0)
                cmbVaccin.SelectedIndex = 0;
        }

        private void BtnAnnulerModif_Click(object sender, RoutedEventArgs e)
        {
            ResetModeAjout();
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // validation vaccin
                if (cmbVaccin.SelectedItem == null)
                {
                    MessageBox.Show("Selectionnez un vaccin.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // validation date
                if (dpDateVaccin.SelectedDate == null)
                {
                    MessageBox.Show("La date est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDateVaccin.SelectedDate.Value > DateTime.Today)
                {
                    MessageBox.Show("La date de vaccination ne peut pas etre dans le futur.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string vaccin = cmbVaccin.SelectedItem.ToString();
                DateTime date = dpDateVaccin.SelectedDate.Value;

                Vaccination nouvelleVac = new Vaccination(animal, vaccin, date);

                if (enModeModification)
                {
                    // modif : supprime l'ancienne + insere la nouvelle
                    vueModele.ModifierVaccination(vaccinationOriginale, nouvelleVac);
                    MessageBox.Show("Vaccination modifiee !");
                    ResetModeAjout();
                }
                else
                {
                    // ajout normal
                    vueModele.AjouterVaccination(nouvelleVac);
                    MessageBox.Show("Vaccination ajoutee !");
                }

                // refresh le DataGrid
                ChargerVaccinations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
