import { TestBed } from '@angular/core/testing';
import { Faq } from './faq';

describe('Faq', () => {
  it('n’ouvre qu’une question à la fois et met à jour aria-expanded', async () => {
    const fixture = TestBed.createComponent(Faq);
    await fixture.whenStable();
    const boutons = () => Array.from((fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>('.faq-q'));
    const ouverts = () => boutons().filter((b) => b.getAttribute('aria-expanded') === 'true').length;

    expect(boutons()).toHaveLength(8);
    expect(boutons()[0].getAttribute('aria-expanded')).toBe('true');

    boutons()[2].click();
    await fixture.whenStable();
    expect(ouverts()).toBe(1);
    expect(boutons()[2].getAttribute('aria-expanded')).toBe('true');

    boutons()[2].click();
    await fixture.whenStable();
    expect(ouverts()).toBe(0);
  });
});
