# Refuge d'Animaux - Application WPF

Application de gestion d'un refuge animalier developpee en C# avec WPF et PostgreSQL.
Projet realise dans le cadre du cours de SGBD (3eme annee, Ecole de Verviers).

---

## Technologies

- **Langage :** C# (.NET Framework 4.7.2)
- **Interface :** WPF (Windows Presentation Foundation)
- **Base de donnees :** PostgreSQL 13
- **Driver BD :** Npgsql 4.1.9
- **Architecture :** MVVM (Model-View-ViewModel)

---

## Fonctionnalites

- Gestion des animaux (chiens et chats) : ajout, modification, suppression, consultation
- Gestion des contacts (benevoles, adoptants, candidats, familles d'accueil)
- Suivi des entrees et sorties du refuge
- Processus d'adoption complet (demande, acceptation, rejet)
- Placement en famille d'accueil
- Suivi des vaccinations
- Gestion des compatibilites animales (chat, chien, enfants, jardin, etc.)
- Identifiants animaux auto-generes (format yymmdd99999)

---

## Base de donnees

- **procedures stockees** (SELECT, INSERT, UPDATE, DELETE, verifications metier)
- Contraintes CHECK, cles etrangeres avec CASCADE, regex sur les formats

Tables principales : `ANIMAL`, `CONTACT`, `ROLE`, `ANI_ENTREE`, `ANI_SORTIE`, `ADOPTION`, `FAMILLE_ACCUEIL`, `VACCINATION`, `VACCIN`, `COMPATIBILITE`, `ANI_COMPATIBILITE`, `ANIMAL_COULEUR`, `PERSONNE_ROLE`

---

## Prerequis

- Windows avec .NET Framework 4.7.2
- PostgreSQL 13 ou superieur
- Visual Studio 2019 ou plus recent

---


## Auteur

Projet realise par **Lallemand Fred** - Ecole de Verviers, 3eme annee (2025-2026)
