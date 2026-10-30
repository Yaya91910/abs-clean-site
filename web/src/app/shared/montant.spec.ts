import { TestBed } from '@angular/core/testing';
import { Montant } from './montant';

describe('Montant', () => {
  it('affiche « — € » sans montant, puis arrive à la valeur finale', async () => {
    const fixture = TestBed.createComponent(Montant);
    fixture.detectChanges();
    const texte = () => (fixture.nativeElement as HTMLElement).textContent!.trim();
    expect(texte()).toBe('— €');

    fixture.componentRef.setInput('valeur', 40);
    fixture.detectChanges();
    expect(texte()).toBe('À partir de 40 €');

    // Passage à 120 € : chiffre animé, mais toujours la bonne valeur à la fin (même sans images d'animation).
    fixture.componentRef.setInput('valeur', 120);
    fixture.detectChanges();
    await new Promise((fin) => setTimeout(fin, 700));
    fixture.detectChanges();
    expect(texte()).toBe('À partir de 120 €');
  });

  it('est masqué aux lecteurs d’écran (le texte final est annoncé ailleurs)', () => {
    const fixture = TestBed.createComponent(Montant);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).getAttribute('aria-hidden')).toBe('true');
  });
});
