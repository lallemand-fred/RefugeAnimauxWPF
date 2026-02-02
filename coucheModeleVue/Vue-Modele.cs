using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using RefugeAnimaux.classeMetier;
using RefugeAnimaux.coucheAccesBD;

namespace RefugeAnimaux.coucheModeleVue
{
    /// <summary>
    /// Vue-Modele principale - fait le lien entre AccesBD et l'interface WPF
    /// </summary>
    public class Vue_Modele : INotifyPropertyChanged
    {
        // ============================================================
        // EVENEMENT INotifyPropertyChanged
        // ============================================================

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// notifie la vue qu'une propriete a change
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ============================================================
        // PROPRIETES PRIVEES
        // ============================================================

        private AccesBD accesBD;

        // collections observables
        private ObservableCollection<Animal> listeAnimaux;
        private ObservableCollection<Contact> listeContacts;

        // elements selectionnes
        private Animal animalSelectionne;
        private Contact contactSelectionne;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        public Vue_Modele()
        {
            // init la couche d'acces aux donnees
            accesBD = new AccesBD();

            // init la sequence d'identifiants depuis la BD
            // sinon ca repart a 1 a chaque demarrage et ca fait des doublons
            string dernierID = accesBD.ObtenirDernierIdentifiantAnimal();
            Animal.InitialiserDepuisBD(dernierID);

            // init les collections observables
            listeAnimaux = new ObservableCollection<Animal>();
            listeContacts = new ObservableCollection<Contact>();

            // charge les donnees au demarrage
            ChargerAnimaux();
            ChargerContacts();
        }

        // ============================================================
        // PROPRIETES PUBLIQUES (avec notification)
        // ============================================================

        /// <summary>
        /// Liste observable des animaux du refuge
        /// </summary>
        public ObservableCollection<Animal> ListeAnimaux
        {
            get { return listeAnimaux; }
            set
            {
                if (listeAnimaux != value)
                {
                    listeAnimaux = value;
                    OnPropertyChanged(nameof(ListeAnimaux));
                }
            }
        }

        /// <summary>
        /// Liste observable des contacts
        /// </summary>
        public ObservableCollection<Contact> ListeContacts
        {
            get { return listeContacts; }
            set
            {
                if (listeContacts != value)
                {
                    listeContacts = value;
                    OnPropertyChanged(nameof(ListeContacts));
                }
            }
        }

        /// <summary>
        /// Animal actuellement selectionne dans l'interface
        /// </summary>
        public Animal AnimalSelectionne
        {
            get { return animalSelectionne; }
            set
            {
                if (animalSelectionne != value)
                {
                    animalSelectionne = value;
                    OnPropertyChanged(nameof(AnimalSelectionne));
                }
            }
        }

        /// <summary>
        /// Contact actuellement selectionne dans l'interface
        /// </summary>
        public Contact ContactSelectionne
        {
            get { return contactSelectionne; }
            set
            {
                if (contactSelectionne != value)
                {
                    contactSelectionne = value;
                    OnPropertyChanged(nameof(ContactSelectionne));
                }
            }
        }

        // ============================================================
        // METHODES DE CHARGEMENT
        // ============================================================

        /// <summary>
        /// Charge la liste des animaux depuis la BD
        /// </summary>
        public void ChargerAnimaux()
        {
            try
            {
                listeAnimaux.Clear();

                // recup les animaux depuis AccesBD
                var animaux = accesBD.ListeAnimaux();

                foreach (var animal in animaux)
                {
                    listeAnimaux.Add(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur chargement animaux: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge la liste des contacts depuis la BD
        /// </summary>
        public void ChargerContacts()
        {
            try
            {
                listeContacts.Clear();

                // recup les contacts depuis AccesBD
                var contacts = accesBD.ListeContacts();

                foreach (var contact in contacts)
                {
                    listeContacts.Add(contact);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur chargement contacts: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES D'AJOUT
        // ============================================================

        /// <summary>
        /// Ajoute un nouvel animal dans la BD et la liste
        /// </summary>
        public void AjouterAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.AjouterAnimal(animal);

                if (resultat > 0)
                {
                    listeAnimaux.Add(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur ajout animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ajoute un nouveau contact dans la BD et la liste
        /// </summary>
        public void AjouterContact(Contact contact)
        {
            try
            {
                // AjouterContact retourne l'ID auto-genere
                int idContact = accesBD.AjouterContact(contact);

                if (idContact > 0)
                {
                    // recharge la liste pour avoir le bon ID
                    ChargerContacts();
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur ajout contact: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE MODIFICATION
        // ============================================================

        /// <summary>
        /// Modifie un animal existant dans la BD
        /// </summary>
        public void ModifierAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.ModifierAnimal(animal);

                if (resultat > 0)
                {
                    // notifie le changement pour rafraichir la vue
                    OnPropertyChanged(nameof(ListeAnimaux));
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur modification animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Modifie un contact existant dans la BD
        /// </summary>
        public void ModifierContact(Contact contact)
        {
            try
            {
                int resultat = accesBD.ModifierContact(contact);

                if (resultat > 0)
                {
                    OnPropertyChanged(nameof(ListeContacts));
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur modification contact: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE SUPPRESSION
        // ============================================================

        /// <summary>
        /// Supprime un animal de la BD et de la liste
        /// </summary>
        public void SupprimerAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.SupprimerAnimal(animal.Identifiant);

                if (resultat > 0)
                {
                    listeAnimaux.Remove(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur suppression animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Supprime un contact de la BD et de la liste
        /// </summary>
        public void SupprimerContact(Contact contact)
        {
            try
            {
                int resultat = accesBD.SupprimerContact(contact.Id);

                if (resultat > 0)
                {
                    listeContacts.Remove(contact);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur suppression contact: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE RECHERCHE
        // ============================================================

        /// <summary>
        /// Recherche un animal par son identifiant (11 chiffres)
        /// </summary>
        public Animal RechercherAnimal(string identifiant)
        {
            try
            {
                return accesBD.ObtenirAnimal(identifiant);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur recherche animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Recherche un contact par son ID
        /// </summary>
        public Contact RechercherContact(int id)
        {
            try
            {
                return accesBD.ObtenirContact(id);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur recherche contact: {ex.Message}", ex);
            }
        }
    }
}
