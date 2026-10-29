import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { Creneau } from './demandes-api';

/**
 * Formulaire de coordonnées (CAHIER-DES-CHARGES.md §8) : mêmes règles que la validation serveur
 * (api/Domain/NouvelleDemande.cs), messages d'erreur et description des champs.
 */

const requis = (c: AbstractControl): ValidationErrors | null => ((c.value ?? '').toString().trim() ? null : { required: true });

const telephoneFr = (c: AbstractControl): ValidationErrors | null =>
  !c.value || /^0[1-79]\d{8}$/.test(c.value.replace(/ /g, '')) ? null : { telephone: true };

const email = (c: AbstractControl): ValidationErrors | null =>
  !c.value || /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(c.value.trim()) ? null : { email: true };

const codePostal = (c: AbstractControl): ValidationErrors | null => (!c.value || /^\d{5}$/.test(c.value.trim()) ? null : { codePostal: true });

export function creerFormulaireCoordonnees() {
  return new FormGroup({
    nom: new FormControl('', { nonNullable: true, validators: [requis, Validators.maxLength(60)] }),
    prenom: new FormControl('', { nonNullable: true, validators: [requis, Validators.maxLength(60)] }),
    telephone: new FormControl('', { nonNullable: true, validators: [requis, telephoneFr] }),
    email: new FormControl('', { nonNullable: true, validators: [requis, email, Validators.maxLength(254)] }),
    adresse: new FormControl('', { nonNullable: true, validators: [requis, Validators.maxLength(200)] }),
    codePostal: new FormControl('', { nonNullable: true, validators: [requis, codePostal] }),
    ville: new FormControl('', { nonNullable: true, validators: [requis, Validators.maxLength(100)] }),
    acces: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(200)] }),
    creneaux: new FormControl<Creneau[]>([], { nonNullable: true }),
    commentaire: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(1000)] }),
    consentement: new FormControl(false, { nonNullable: true, validators: [Validators.requiredTrue] }),
    /** Champ piège (honeypot) : toujours vide pour un humain. */
    siteWeb: new FormControl('', { nonNullable: true }),
  });
}

export type FormulaireCoordonnees = ReturnType<typeof creerFormulaireCoordonnees>;

const MESSAGES = {
  nom: { required: 'Le nom est requis.', maxlength: 'Le nom ne doit pas dépasser 60 caractères.' },
  prenom: { required: 'Le prénom est requis.', maxlength: 'Le prénom ne doit pas dépasser 60 caractères.' },
  telephone: { required: 'Le téléphone est requis.', telephone: 'Numéro de téléphone français invalide (ex. 06 12 34 56 78).' },
  email: { required: "L'e-mail est requis.", email: 'Adresse e-mail invalide.' },
  adresse: { required: "L'adresse d'intervention est requise.", maxlength: "L'adresse ne doit pas dépasser 200 caractères." },
  codePostal: { required: 'Le code postal est requis.', codePostal: 'Le code postal doit contenir 5 chiffres.' },
  ville: { required: 'La ville est requise.', maxlength: 'La ville ne doit pas dépasser 100 caractères.' },
  acces: { maxlength: "L'accès ne doit pas dépasser 200 caractères." },
  commentaire: { maxlength: 'Le commentaire ne doit pas dépasser 1000 caractères.' },
  consentement: { required: 'Votre accord est nécessaire pour que nous puissions vous recontacter.' },
} satisfies Record<string, Record<string, string>>;

export type ChampAvecErreur = keyof typeof MESSAGES;

/** Message à afficher pour les erreurs d'un champ ; une erreur renvoyée par le serveur est affichée telle quelle. */
export function messageErreur(champ: ChampAvecErreur, erreurs: ValidationErrors): string {
  if (erreurs['serveur']) return erreurs['serveur'];
  const cle = Object.keys(erreurs)[0];
  const messages: Record<string, string> = MESSAGES[champ];
  return messages[cle === 'requiredTrue' ? 'required' : cle] ?? 'Valeur invalide.';
}

export const CRENEAUX: { id: Creneau; libelle: string }[] = [
  { id: 'semaineMatin', libelle: 'Semaine · matin' },
  { id: 'semaineApresMidi', libelle: 'Semaine · après-midi' },
  { id: 'samedi', libelle: 'Samedi' },
];

interface ChampTexte {
  nom: ChampAvecErreur;
  libelle: string;
  type: 'text' | 'tel' | 'email';
  exemple: string;
  autocomplete: string;
  inputmode: 'tel' | 'email' | 'numeric' | null;
  pleineLargeur: boolean;
}

/** Champs texte de la fenêtre, dans l'ordre d'affichage de la maquette. */
export const CHAMPS_TEXTE = [
  { nom: 'nom', libelle: 'Nom', type: 'text', exemple: 'Dupont', autocomplete: 'family-name', inputmode: null, pleineLargeur: false },
  { nom: 'prenom', libelle: 'Prénom', type: 'text', exemple: 'Marie', autocomplete: 'given-name', inputmode: null, pleineLargeur: false },
  { nom: 'telephone', libelle: 'Téléphone', type: 'tel', exemple: '06 12 34 56 78', autocomplete: 'tel', inputmode: 'tel', pleineLargeur: false },
  { nom: 'email', libelle: 'E-mail', type: 'email', exemple: 'marie@exemple.fr', autocomplete: 'email', inputmode: 'email', pleineLargeur: false },
  { nom: 'adresse', libelle: "Adresse d'intervention", type: 'text', exemple: '12 rue des Lilas', autocomplete: 'street-address', inputmode: null, pleineLargeur: true },
  { nom: 'codePostal', libelle: 'Code postal', type: 'text', exemple: '91000', autocomplete: 'postal-code', inputmode: 'numeric', pleineLargeur: false },
  { nom: 'ville', libelle: 'Ville', type: 'text', exemple: 'Évry-Courcouronnes', autocomplete: 'address-level2', inputmode: null, pleineLargeur: false },
  { nom: 'acces', libelle: 'Accès (étage, digicode…)', type: 'text', exemple: '3e étage, code 1234B', autocomplete: 'off', inputmode: null, pleineLargeur: true },
  { nom: 'commentaire', libelle: 'Commentaire (facultatif)', type: 'text', exemple: "Précisions utiles : vitres difficiles d'accès, animaux…", autocomplete: 'off', inputmode: null, pleineLargeur: true },
] as const satisfies readonly ChampTexte[];
