using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireArrivee : Window
    {
        // collections recues du vue-modele
        private ObservableCollection<Animal> animaux;
        private ObservableCollection<Contact> contacts;
        private List<Adoption> adoptions;

        // resultats a retourner
        public Animal ResultAnimal { get; private set; }
        public AnimalEntree ResultEntree { get; private set; }
        public bool EstNouvelAnimal { get; private set; }
        public Contact NouveauContact { get; private set; }

        public FormulaireArrivee(ObservableCollection<Animal> animaux, ObservableCollection<Contact> contacts, List<Adoption> adoptions)
        {
            InitializeComponent();

            this.animaux = animaux;
            this.contacts = contacts;
            this.adoptions = adoptions;

            // remplit la combobox animaux (format: "Identifiant - Nom (Type)")
            foreach (var animal in animaux)
            {
                cmbAnimalExistant.Items.Add(new ComboBoxItem
                {
                    Content = $"{animal.Identifiant} - {animal.Nom} ({animal.Type})",
                    Tag = animal
                });
            }

            // remplit la combobox contacts (format: "Nom Prenom")
            RemplirComboContacts();

            // date d'entree = aujourd'hui par defaut
            dpDateEntree.SelectedDate = DateTime.Today;

            // "Nouvel animal" est selectionne par defaut, donc on affiche les champs
            AfficherChampsNouvelAnimal(true);
        }

        // recharge la combobox contacts (apres ajout d'un nouveau)
        private void RemplirComboContacts()
        {
            cmbContact.Items.Clear();
            foreach (var contact in contacts)
            {
                cmbContact.Items.Add(new ComboBoxItem
                {
                    Content = $"{contact.Nom} {contact.Prenom}",
                    Tag = contact
                });
            }
        }

        // affiche/masque les champs selon le mode (nouvel animal ou existant)
        private void AfficherChampsNouvelAnimal(bool nouvelAnimal)
        {
            // check si les controles sont initialises (evite le crash au demarrage)
            if (lblNom == null) return;

            Visibility visibleNouvel = nouvelAnimal ? Visibility.Visible : Visibility.Collapsed;
            Visibility visibleExistant = nouvelAnimal ? Visibility.Collapsed : Visibility.Visible;

            // champs nouvel animal
            lblNom.Visibility = visibleNouvel;
            txtNom.Visibility = visibleNouvel;
            lblType.Visibility = visibleNouvel;
            cmbType.Visibility = visibleNouvel;
            lblSexe.Visibility = visibleNouvel;
            cmbSexe.Visibility = visibleNouvel;
            lblDateNaissance.Visibility = visibleNouvel;
            dpDateNaissance.Visibility = visibleNouvel;

            // combobox animal existant
            lblAnimalExistant.Visibility = visibleExistant;
            cmbAnimalExistant.Visibility = visibleExistant;
        }

        private void RbNouvelAnimal_Checked(object sender, RoutedEventArgs e)
        {
            AfficherChampsNouvelAnimal(true);
        }

        private void RbAnimalExistant_Checked(object sender, RoutedEventArgs e)
        {
            AfficherChampsNouvelAnimal(false);
        }

        private void BtnNouveauContact_Click(object sender, RoutedEventArgs e)
        {
            var formulaire = new FormulaireContact();
            formulaire.Owner = this;

            if (formulaire.ShowDialog() == true)
            {
                // stocke le nouveau contact
                NouveauContact = formulaire.ResultContact;

                // ajoute a la combobox et le selectionne
                var item = new ComboBoxItem
                {
                    Content = $"{NouveauContact.Nom} {NouveauContact.Prenom}",
                    Tag = NouveauContact
                };
                cmbContact.Items.Add(item);
                cmbContact.SelectedItem = item;
            }
        }

        // quand on change de raison, faut verifier si c'est retour_adoption pour verrouiller le contact
        private void CmbRaison_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRaison.SelectedItem == null || cmbContact == null) return;

            string raison = ((ComboBoxItem)cmbRaison.SelectedItem).Content.ToString();

            if (raison == "retour_adoption")
            {
                MettreAJourContactRetourAdoption();
            }
            else
            {
                // on deverrouille le contact et le bouton nouveau contact
                cmbContact.IsEnabled = true;
                btnNouveauContact.IsEnabled = true;
            }
        }

        // quand on change d'animal existant et que la raison est retour_adoption
        private void CmbAnimalExistant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRaison == null || cmbRaison.SelectedItem == null) return;

            string raison = ((ComboBoxItem)cmbRaison.SelectedItem).Content.ToString();
            if (raison == "retour_adoption")
            {
                MettreAJourContactRetourAdoption();
            }
        }

        // cherche l'adoption acceptee pour l'animal et verrouille le contact
        private void MettreAJourContactRetourAdoption()
        {
            // faut un animal existant selectionne
            if (cmbAnimalExistant.SelectedItem == null)
            {
                cmbContact.IsEnabled = true;
                btnNouveauContact.IsEnabled = true;
                return;
            }

            Animal animalSelect = (Animal)((ComboBoxItem)cmbAnimalExistant.SelectedItem).Tag;

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
                // on selectionne le contact de l'adoption et on verrouille
                foreach (ComboBoxItem item in cmbContact.Items)
                {
                    Contact c = item.Tag as Contact;
                    if (c != null && c.Id == adoptionAcceptee.ContactId)
                    {
                        cmbContact.SelectedItem = item;
                        cmbContact.IsEnabled = false;
                        btnNouveauContact.IsEnabled = false;
                        return;
                    }
                }
            }

            // pas d'adoption acceptee trouvee, on laisse le choix libre
            cmbContact.IsEnabled = true;
            btnNouveauContact.IsEnabled = true;
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // validation selon le mode
                if (rbNouvelAnimal.IsChecked == true)
                {
                    // mode nouvel animal
                    string nom = txtNom.Text.Trim();
                    if (string.IsNullOrWhiteSpace(nom))
                    {
                        MessageBox.Show("Le nom de l'animal est obligatoire.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtNom.Focus();
                        return;
                    }

                    if (cmbType.SelectedItem == null)
                    {
                        MessageBox.Show("Le type est obligatoire.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (cmbSexe.SelectedItem == null)
                    {
                        MessageBox.Show("Le sexe est obligatoire.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (dpDateNaissance.SelectedDate == null)
                    {
                        MessageBox.Show("La date de naissance est obligatoire.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // cree le nouvel animal
                    string type = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();
                    char sexe = ((ComboBoxItem)cmbSexe.SelectedItem).Content.ToString()[0];
                    DateTime dateNaissance = dpDateNaissance.SelectedDate.Value;

                    ResultAnimal = new Animal(nom, type, sexe, dateNaissance);
                    EstNouvelAnimal = true;
                }
                else
                {
                    // mode animal existant
                    if (cmbAnimalExistant.SelectedItem == null)
                    {
                        MessageBox.Show("Selectionnez un animal.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    ResultAnimal = (Animal)((ComboBoxItem)cmbAnimalExistant.SelectedItem).Tag;
                    EstNouvelAnimal = false;
                }

                // validation contact
                if (cmbContact.SelectedItem == null)
                {
                    MessageBox.Show("Selectionnez un contact.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // validation raison
                if (cmbRaison.SelectedItem == null)
                {
                    MessageBox.Show("Selectionnez une raison.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // validation date entree
                if (dpDateEntree.SelectedDate == null)
                {
                    MessageBox.Show("La date d'entree est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // recup le contact selectionne
                Contact contactSelectionne = (Contact)((ComboBoxItem)cmbContact.SelectedItem).Tag;
                string raison = ((ComboBoxItem)cmbRaison.SelectedItem).Content.ToString();
                DateTime dateEntree = dpDateEntree.SelectedDate.Value;

                // cree l'entree
                // on met ContactId a 0 pour l'instant, Vue-Modele le mettra a jour si nouveau contact
                int contactId = NouveauContact != null ? 0 : contactSelectionne.Id;
                ResultEntree = new AnimalEntree(ResultAnimal.Identifiant, dateEntree, raison, contactId);

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
