using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireSortie : Window
    {
        // collections recues du vue-modele
        private ObservableCollection<Animal> animaux;
        private ObservableCollection<Contact> contacts;
        private List<Adoption> adoptions;

        // resultats a retourner
        public Animal ResultAnimal { get; private set; }
        public AnimalSortie ResultSortie { get; private set; }
        public DateTime? DateDeces { get; private set; }

        public FormulaireSortie(ObservableCollection<Animal> animaux, ObservableCollection<Contact> contacts, List<Adoption> adoptions)
        {
            InitializeComponent();

            this.animaux = animaux;
            this.contacts = contacts;
            this.adoptions = adoptions;

            // remplit la combobox animaux (format: "Identifiant - Nom (Type)")
            foreach (var animal in animaux)
            {
                cmbAnimal.Items.Add(new ComboBoxItem
                {
                    Content = $"{animal.Identifiant} - {animal.Nom} ({animal.Type})",
                    Tag = animal
                });
            }

            // remplit les combobox contacts sauf adoption (celle-la est geree dynamiquement)
            RemplirComboContacts(cmbContactFamille);
            RemplirComboContacts(cmbContactProprietaire);

            // date de sortie = aujourd'hui par defaut
            dpDateSortie.SelectedDate = DateTime.Today;
        }

        private void RemplirComboContacts(ComboBox combo)
        {
            foreach (var contact in contacts)
            {
                combo.Items.Add(new ComboBoxItem
                {
                    Content = $"{contact.Nom} {contact.Prenom}",
                    Tag = contact
                });
            }
        }

        // quand on change d'animal, faut mettre a jour le contact adoption si besoin
        private void CmbAnimal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbAdoption.IsChecked == true)
            {
                MettreAJourContactAdoption();
            }
        }

        // cherche l'adoption acceptee pour l'animal et verrouille le bon contact
        private void MettreAJourContactAdoption()
        {
            cmbContactAdoption.Items.Clear();
            cmbContactAdoption.IsEnabled = true;

            if (cmbAnimal.SelectedItem == null) return;

            Animal animalSelect = (Animal)((ComboBoxItem)cmbAnimal.SelectedItem).Tag;

            // cherche l'adoption acceptee pour cet animal
            Adoption adoptionAcceptee = null;
            foreach (var adoption in adoptions)
            {
                if (adoption.AnimalId.Trim() == animalSelect.Identifiant.Trim() &&
                    adoption.Statut.Trim().ToLower() == "acceptee")
                {
                    adoptionAcceptee = adoption;
                    break;
                }
            }

            if (adoptionAcceptee != null)
            {
                // on met que le contact de l'adoption acceptee et on verrouille
                foreach (var contact in contacts)
                {
                    if (contact.Id == adoptionAcceptee.ContactId)
                    {
                        cmbContactAdoption.Items.Add(new ComboBoxItem
                        {
                            Content = $"{contact.Nom} {contact.Prenom}",
                            Tag = contact
                        });
                        cmbContactAdoption.SelectedIndex = 0;
                        cmbContactAdoption.IsEnabled = false;
                        break;
                    }
                }
            }
            else
            {
                // pas d'adoption acceptee, combo vide (AccesBD bloquera de toute facon)
                // on met un message pour que l'utilisateur comprenne
                cmbContactAdoption.Items.Add(new ComboBoxItem
                {
                    Content = "Aucune adoption acceptee",
                    IsEnabled = false
                });
            }
        }

        private void RbRaison_Checked(object sender, RoutedEventArgs e)
        {
            // check si les controles sont initialises
            if (cmbContactAdoption == null) return;

            // masque tout
            cmbContactAdoption.Visibility = Visibility.Collapsed;
            cmbContactFamille.Visibility = Visibility.Collapsed;
            cmbContactProprietaire.Visibility = Visibility.Collapsed;
            lblDecesInfo.Visibility = Visibility.Collapsed;

            // affiche selon la raison selectionnee
            if (rbAdoption.IsChecked == true)
            {
                cmbContactAdoption.Visibility = Visibility.Visible;
                // filtre le contact selon l'adoption acceptee
                MettreAJourContactAdoption();
            }
            else if (rbFamilleAccueil.IsChecked == true)
            {
                cmbContactFamille.Visibility = Visibility.Visible;
            }
            else if (rbDeces.IsChecked == true)
            {
                lblDecesInfo.Visibility = Visibility.Visible;
            }
            else if (rbRetourProprietaire.IsChecked == true)
            {
                cmbContactProprietaire.Visibility = Visibility.Visible;
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // validation animal
                if (cmbAnimal.SelectedItem == null)
                {
                    MessageBox.Show("Selectionnez un animal.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ResultAnimal = (Animal)((ComboBoxItem)cmbAnimal.SelectedItem).Tag;

                // validation raison
                if (rbAdoption.IsChecked != true && rbFamilleAccueil.IsChecked != true &&
                    rbDeces.IsChecked != true && rbRetourProprietaire.IsChecked != true)
                {
                    MessageBox.Show("Selectionnez une raison de sortie.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // validation date
                if (dpDateSortie.SelectedDate == null)
                {
                    MessageBox.Show("La date de sortie est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime dateSortie = dpDateSortie.SelectedDate.Value;
                string raison = "";
                int contactId = 0;
                DateDeces = null;

                // selon la raison, on recup le contact et on valide
                if (rbAdoption.IsChecked == true)
                {
                    raison = "adoption";
                    if (cmbContactAdoption.SelectedItem == null)
                    {
                        MessageBox.Show("Selectionnez le contact adoptant.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    contactId = ((Contact)((ComboBoxItem)cmbContactAdoption.SelectedItem).Tag).Id;
                }
                else if (rbFamilleAccueil.IsChecked == true)
                {
                    raison = "famille_accueil";
                    if (cmbContactFamille.SelectedItem == null)
                    {
                        MessageBox.Show("Selectionnez le contact famille d'accueil.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    contactId = ((Contact)((ComboBoxItem)cmbContactFamille.SelectedItem).Tag).Id;
                }
                else if (rbDeces.IsChecked == true)
                {
                    raison = "deces_animal";
                    // pas de contact pour un deces, on met 0
                    // mais AnimalSortie.ContactId refuse 0... faut voir
                    // en fait AccesBD gere ca, on va mettre un contact fictif ou gerer dans AccesBD
                    // pour l'instant on va demander un contact quand meme (celui qui a constate le deces)
                    DateDeces = dateSortie;

                    // on met contactId = 1 par defaut (le refuge) ou on demande pas
                    // vu que la validation de AnimalSortie refuse 0, on va devoir adapter
                    // pour l'instant, mettons qu'on demande pas et on gere dans le constructeur
                    contactId = 1; // contact refuge par defaut
                }
                else if (rbRetourProprietaire.IsChecked == true)
                {
                    raison = "retour_proprietaire";
                    if (cmbContactProprietaire.SelectedItem == null)
                    {
                        MessageBox.Show("Selectionnez le contact proprietaire.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    contactId = ((Contact)((ComboBoxItem)cmbContactProprietaire.SelectedItem).Tag).Id;
                }

                // cree la sortie
                ResultSortie = new AnimalSortie(ResultAnimal.Identifiant, dateSortie, raison, contactId);

                this.DialogResult = true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
