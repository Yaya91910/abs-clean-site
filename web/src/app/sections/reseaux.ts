import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Liens et identifiants des comptes : À FOURNIR par le gérant (CAHIER-DES-CHARGES.md §18). */
const RESEAUX = {
  tiktok: { url: 'https://www.tiktok.com/', identifiant: '[@compte TikTok]' },
  instagram: { url: 'https://www.instagram.com/', identifiant: '[@compte Instagram]' },
};

@Component({
  selector: 'abs-reseaux',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section reseaux">
      <div class="container">
        <div class="glass social-wrap">
          <div class="head">
            <span class="kicker">Réseaux sociaux</span>
            <h2 class="title">Rejoignez-nous sur les réseaux</h2>
            <p class="sub">Suivez nos interventions, nos avant/après et nos conseils d'entretien.</p>
          </div>
          <div class="social-row">
            <a class="social-card tiktok" [href]="reseaux.tiktok.url" target="_blank" rel="noopener noreferrer" aria-label="ABS clean sur TikTok (nouvel onglet)">
              <span class="social-ico" aria-hidden="true"><svg width="26" height="26" viewBox="0 0 24 24" fill="none"><path d="M9 18V6l10-2v12" stroke="#FFFFFF" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /><circle cx="6.5" cy="18" r="2.5" stroke="#FFFFFF" stroke-width="1.8" /><circle cx="16.5" cy="16" r="2.5" stroke="#FFFFFF" stroke-width="1.8" /></svg></span>
              <span class="social-txt"><span class="social-name">TikTok</span><span class="social-handle">{{ reseaux.tiktok.identifiant }}</span></span>
              <span class="social-go" aria-hidden="true"><svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M7 17L17 7M9 7h8v8" stroke="#FFFFFF" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg></span>
            </a>
            <a class="social-card insta" [href]="reseaux.instagram.url" target="_blank" rel="noopener noreferrer" aria-label="ABS clean sur Instagram (nouvel onglet)">
              <span class="social-ico" aria-hidden="true"><svg width="26" height="26" viewBox="0 0 24 24" fill="none"><path d="M4 8.5A2.5 2.5 0 016.5 6h2l1.4-2h4.2l1.4 2h2A2.5 2.5 0 0120 8.5v8a2.5 2.5 0 01-2.5 2.5h-11A2.5 2.5 0 014 16.5v-8z" stroke="#FFFFFF" stroke-width="1.8" stroke-linejoin="round" /><circle cx="12" cy="12.5" r="3.4" stroke="#FFFFFF" stroke-width="1.8" /></svg></span>
              <span class="social-txt"><span class="social-name">Instagram</span><span class="social-handle">{{ reseaux.instagram.identifiant }}</span></span>
              <span class="social-go" aria-hidden="true"><svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M7 17L17 7M9 7h8v8" stroke="#FFFFFF" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg></span>
            </a>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .reseaux { padding-top: 24px; }
    .social-wrap { width: 100%; padding: clamp(32px, 6vw, 52px) clamp(18px, 4vw, 40px); display: flex; flex-direction: column; align-items: center; gap: 30px; position: relative; overflow: hidden; }
    .social-row { width: 100%; max-width: 640px; display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
    @media (max-width: 560px) { .social-row { grid-template-columns: 1fr; } }
    .social-card { position: relative; overflow: hidden; display: flex; align-items: center; gap: 14px; min-height: 84px; padding: 14px 14px 14px 16px; border-radius: 22px; color: #FFFFFF; text-align: left; transition: transform .2s ease, box-shadow .2s ease; }
    .social-card:hover { transform: translateY(-4px); }
    .social-card.tiktok { background: linear-gradient(135deg, var(--ink) 0%, #183A6A 55%, #3D86D8 100%); box-shadow: 0 18px 34px -16px rgba(21, 52, 95, 0.6); }
    .social-card.insta { background: linear-gradient(135deg, var(--accent2) -10%, var(--accent) 50%, #1F55B5 100%); box-shadow: 0 18px 34px -16px var(--accent-shadow); }
    .social-card::after { content: ""; position: absolute; top: -40%; right: -10%; width: 140px; height: 140px; border-radius: 50%; background: rgba(255, 255, 255, 0.12); pointer-events: none; }
    .social-ico { width: 52px; height: 52px; border-radius: 16px; background: rgba(255, 255, 255, 0.16); border: 1px solid rgba(255, 255, 255, 0.25); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .social-txt { display: flex; flex-direction: column; gap: 3px; flex: 1; min-width: 0; }
    .social-name { font-size: 18px; font-weight: 800; letter-spacing: -0.01em; }
    .social-handle { font-size: 13px; color: rgba(255, 255, 255, 0.78); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .social-go { position: relative; z-index: 1; width: 40px; height: 40px; border-radius: 999px; background: rgba(255, 255, 255, 0.2); display: flex; align-items: center; justify-content: center; flex-shrink: 0; transition: transform .2s ease, background .2s ease; }
    .social-card:hover .social-go { transform: rotate(45deg); background: rgba(255, 255, 255, 0.32); }
  `,
})
export class Reseaux {
  protected readonly reseaux = RESEAUX;
}
