import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('affiche le titre du héros', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const page = fixture.nativeElement as HTMLElement;

    expect(page.querySelector('h1')?.textContent).toContain('La transparence');
  });

  it('contient les ancres de navigation dans l’ordre de la maquette', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const page = fixture.nativeElement as HTMLElement;

    const ids = Array.from(page.querySelectorAll('section[id]')).map((s) => s.id);
    expect(ids).toEqual(['qui-sommes-nous', 'prestations', 'tarifs', 'faq', 'devis', 'parrainage']);
    const ancres = Array.from(page.querySelectorAll<HTMLAnchorElement>('nav a[href^="/#"]'));
    expect(ancres.length).toBeGreaterThan(0);
    for (const lien of ancres) {
      expect(ids).toContain(lien.getAttribute('href')!.slice(2));
    }
  });
});
