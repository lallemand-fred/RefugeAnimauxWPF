using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireAdoption : Window
    {
        // collections recues du vue-modele
        private ObservableCollection<Animal> animaux;
        private ObservableCollection<Contact> contacts;

        // adoption d'origine (null = mode ajout)
        private Adoption adoptionOriginal;

        // resultats a retourner
        public Adoption ResultAdoption { get; private set; }
        public bool EstModification { get; private set; }

        // mode ajout : nouvelle demande
        public FormulaireAdoption(ObservableCollection<Animal> animaux, ObservableCollection<Contact> contacts)
        {
            InitializeComponent();

            this.animaux = animaux;
            this.contacts = contacts;
            this.adoptionOriginal = null;
            this.EstModification = false;

            this.Title = "Nouvelle demande d'adoption";

            // remplit les combobox
            RemplirComboAnimaux();
            RemplirComboContacts();

            // date = aujourd'hui
            dpDateDemande.SelectedDate = DateTime.Today;

            // cache le statut (mode ajout = toujours "demande")
            grpStatut.Visibility = Visibility.Collapsed;
        }

        // mode modification : changer le statut
        public FormulaireAdoption(Adoption adoption, ObservableCollection<Animal> animaux, ObservableCollection<Contact> contacts)
        {
            InitializeComponent();

            this.animaux = animaux;
            this.contacts = contacts;
            this.adoptionOriginal = adoption;
            this.EstModification = true;

            this.Title = "Modifier demande d'adoption";

            // remplit les combobox
            RemplirComboAnimaux();
            RemplirComboContacts();

            // preselectionne l'animal et le contact
            SelectionnerAnimal(adoption.AnimalId);
            SelectionnerContact(adoption.ContactId);

            // affiche la date
            dpDateDemande.SelectedDate = adoption.DateDemande;

            // desactive les champs (on peut seulement changer le statut)
            cmbAnimal.IsEnabled = false;
            cmbContact.IsEnabled = false;
            dpDateDemande.IsEnabled = false;

            // affiche le statut et preselectionne
            grpStatut.Visibility = Visibility.Visible;
            SelectionnerStatut(adoption.Statut);

            btnEnregistrer.Content = "Modifier statut";
        }

        private void RemplirComboAnimaux()
        {
            foreach (var animal in animaux)
            {
                cmbAnimal.Items.Add(new ComboBoxItem
                {
                    Content = $"{animal.Identifiant} - {animal.Nom} ({animal.Type})",
                    Tag = animal
                });
            }
        }

        private void RemplirComboContacts()
        {
            foreach (var contact in contacts)
            {
                cmbContact.Items.Add(new ComboBoxItem
                {
                    Content = $"{contact.Nom} {contact.Prenom}",
                    Tag = contact
                });
            }
        }

        private void SelectionnerAnimal(string animalId)
        {
            foreach (ComboBoxItem item in cmbAnimal.Items)
            {
                Animal a = (Animal)item.Tag;
                if (a.Identifiant == animalId)
                {
                    cmbAnimal.SelectedItem = item;
                    break;
                }
            }
        }

        private void SelectionnerContact(int contactId)
        {
            foreach (ComboBoxItem item in cmbContact.Items)
            {
                Contact c = (Contact)item.Tag;
                if (c.Id == contactId)
                {
                    cmbContact.SelectedItem = item;
                    break;
                }
            }
        }

        private void SelectionnerStatut(string statut)
        {
            foreach (ComboBoxItem item in cmbStatut.Items)
            {
                if (item.Content.ToString() == statut)
                {
                    cmbStatut.SelectedItem = item;
                    break;
                }
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

                // validation contact
                if (cmbContact.SelectedItem == null)
                {
                    MessageBox.Show("Selectionnez un contact.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // validation date
                if (dpDateDemande.SelectedDate == null)
                {
                    MessageBox.Show("La date est obligatoire.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Animal animal = (Animal)((ComboBoxItem)cmbAnimal.SelectedItem).Tag;
                Contact contact = (Contact)((ComboBoxItem)cmbContact.SelectedItem).Tag;
                DateTime dateDemande = dpDateDemande.SelectedDate.Value;

                if (EstModification)
                {
                    // mode modification : on garde l'adoption originale et change le statut
                    if (cmbStatut.SelectedItem == null)
                    {
                        MessageBox.Show("Selectionnez un statut.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string nouveauStatut = ((ComboBoxItem)cmbStatut.SelectedItem).Content.ToString();

                    // constructeur type avec objets complets
                    ResultAdoption = new Adoption(animal, contact, adoptionOriginal.DateDemande);
                    ResultAdoption.Statut = nouveauStatut;
                }
                else
                {
                    // mode ajout : constructeur type avec objets complets
                    ResultAdoption = new Adoption(animal, contact, dateDemande);
                }

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
