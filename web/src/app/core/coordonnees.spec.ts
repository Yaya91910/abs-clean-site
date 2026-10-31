import { creerFormulaireCoordonnees, messageErreur } from './coordonnees';

describe('Formulaire de coordonnées', () => {
  it('refuse les caractères invisibles qui changent la lecture d’un nom ou d’un numéro', () => {
    const form = creerFormulaireCoordonnees();
    for (const [champ, valeur] of [
      ['nom', 'Martin‮enitram'],
      ['telephone', '06 12 34​ 56 78'],
      ['ville', 'Évry Nouveau devis'],
      ['prenom', 'Léa\uD83D'],
    ] as const) {
      const control = form.controls[champ];
      control.setValue(valeur);
      expect(control.errors?.['caracteres']).toBe(true);
      expect(messageErreur(champ, control.errors!)).toBe('Caractères non autorisés.');
    }
  });

  it('garde les retours à la ligne et les émojis du commentaire', () => {
    const commentaire = creerFormulaireCoordonnees().controls.commentaire;
    commentaire.setValue('Famille 👨‍👩‍👧 à la maison.\nMerci 🙏');
    expect(commentaire.errors).toBeNull();
    commentaire.setValue('Merci‮ !');
    expect(commentaire.errors?.['caracteres']).toBe(true);
  });
});
