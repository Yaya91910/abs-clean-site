import { ChangeDetectionStrategy, Component, DOCUMENT, inject, signal } from '@angular/core';

/**
 * Section parrainage (CLAUDE.md §9 bis). Le code et le QR affichés sont un exemple :
 * chaque client reçoit les siens après sa demande. Les boutons partagent le lien du devis.
 */
@Component({
  selector: 'abs-parrainage',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="parrainage" class="section">
      <div class="container">
        <div class="glass ref-wrap">
          <div class="ref-text">
            <span class="kicker">Parrainage</span>
            <h2 class="title">Parrainez vos proches, <br /><span class="grad-text">économisez.</span></h2>
            <div class="ref-offer">
              <span class="ref-badge">−10&nbsp;%</span>
              <span class="ref-offer-txt">sur votre prochaine intervention <strong>pour chaque nouveau client</strong> que vous nous recommandez.</span>
            </div>
            <ol class="ref-steps">
              <li><span class="ref-n" aria-hidden="true">1</span><span>Partagez votre code ou votre QR code.</span></li>
              <li><span class="ref-n" aria-hidden="true">2</span><span>Votre proche fait son devis avec votre code.</span></li>
              <li><span class="ref-n" aria-hidden="true">3</span><span>Dès sa première intervention, vos −10&nbsp;% sont activés.</span></li>
            </ol>
            <div class="ref-share">
              <button type="button" class="share-btn primary" (click)="copier()">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="8" y="8" width="12" height="12" rx="2.5" stroke="currentColor" stroke-width="1.8" /><path d="M16 8V6.5A2.5 2.5 0 0013.5 4h-7A2.5 2.5 0 004 6.5v7A2.5 2.5 0 006.5 16H8" stroke="currentColor" stroke-width="1.8" /></svg>
                <span aria-live="polite">{{ copie() ? 'Copié ✓' : 'Copier le lien' }}</span>
              </button>
              <a class="share-btn" [href]="lienWhatsApp" target="_blank" rel="noopener noreferrer">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 20l1.3-3.9A8 8 0 1112 20a8 8 0 01-4-1.1L4 20z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M9 9.5c.3 2 2.5 4.2 4.5 4.5l1.2-1.1 1.8.9c-.2 1.1-1.1 1.7-2 1.7-3 0-6.5-3.5-6.5-6.5 0-.9.6-1.8 1.7-2l.9 1.8L9 9.5z" fill="currentColor" /></svg>
                <span>WhatsApp</span>
              </a>
              <a class="share-btn" [href]="lienSms">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 6.5A2.5 2.5 0 016.5 4h11A2.5 2.5 0 0120 6.5v8a2.5 2.5 0 01-2.5 2.5H10l-4 3v-3h0A2.5 2.5 0 014 14.5v-8z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M8 10.5h.01M12 10.5h.01M16 10.5h.01" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" /></svg>
                <span>SMS</span>
              </a>
              <a class="share-btn" [href]="lienEmail">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2.5" stroke="currentColor" stroke-width="1.8" /><path d="M4 7l8 6 8-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                <span>E-mail</span>
              </a>
              @if (partageNatif) {
                <button type="button" class="share-btn" (click)="partager()">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 15V4M8 8l4-4 4 4M5 13v5a2 2 0 002 2h10a2 2 0 002-2v-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                  <span>Partager…</span>
                </button>
              }
            </div>
          </div>
          <div class="ref-card">
            <div class="ref-card-glow" aria-hidden="true"></div>
            <span class="ref-card-label">Votre code personnel</span>
            <span class="ref-code">ABS-MARIE7</span>
            <div class="qr-box"><img src="qr-exemple.svg" alt="QR code de parrainage (exemple)" width="160" height="160" loading="lazy" /></div>
            <span class="ref-card-hint">Scannez pour obtenir un devis</span>
            <span class="ref-card-note">Exemple — chaque client reçoit son propre code après sa demande.</span>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .ref-wrap { width: 100%; padding: clamp(26px, 5vw, 48px); display: flex; flex-wrap: wrap; align-items: center; gap: clamp(24px, 4vw, 48px); text-align: left; position: relative; overflow: hidden; }
    .ref-text { flex: 1 1 380px; min-width: 0; display: flex; flex-direction: column; align-items: flex-start; gap: 18px; }
    .ref-text .title { margin-top: 12px; }
    .ref-offer { display: flex; align-items: center; gap: 14px; padding: 14px 16px; border-radius: 18px; background: linear-gradient(135deg, rgba(76, 155, 224, 0.12), rgba(47, 111, 214, 0.10)); border: 1px solid rgba(47, 111, 214, 0.18); }
    .ref-badge { flex-shrink: 0; padding: 10px 14px; border-radius: 14px; background: linear-gradient(135deg, var(--accent2), var(--accent) 55%, #1F55B5); color: #FFFFFF; font-size: 26px; font-weight: 800; letter-spacing: -0.02em; box-shadow: 0 12px 24px -10px var(--accent-shadow); white-space: nowrap; }
    .ref-offer-txt { font-size: 15px; line-height: 1.5; color: var(--ink); }
    .ref-steps { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 10px; }
    .ref-steps li { display: flex; align-items: center; gap: 12px; font-size: 15px; color: #3C4A5C; }
    .ref-n { width: 28px; height: 28px; border-radius: 999px; background: #E8F1FF; color: var(--accent); font-size: 13px; font-weight: 700; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .ref-share { display: flex; flex-wrap: wrap; gap: 10px; }
    .share-btn { white-space: nowrap; display: inline-flex; align-items: center; gap: 8px; min-height: 44px; padding: 0 16px; border-radius: 999px; border: 1px solid #D7E0EA; background: rgba(255, 255, 255, 0.9); color: var(--ink); font-family: inherit; font-size: 14px; font-weight: 600; cursor: pointer; transition: transform .15s ease, box-shadow .15s ease, background .15s ease; }
    .share-btn:hover { transform: translateY(-2px); box-shadow: 0 10px 20px -12px rgba(21, 52, 95, 0.4); background: #FFFFFF; }
    .share-btn.primary { background: linear-gradient(135deg, var(--accent), #1F55B5); color: #FFFFFF; border-color: transparent; box-shadow: 0 12px 22px -12px var(--accent-shadow); }
    .ref-card { position: relative; overflow: hidden; flex: 0 1 300px; min-width: 240px; margin: 0 auto; padding: 26px 22px 22px; border-radius: 26px; background: linear-gradient(150deg, var(--ink) 0%, #183A6A 55%, #1F55B5 120%); display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; box-shadow: 0 30px 60px -28px rgba(21, 52, 95, 0.6); }
    .ref-card-glow { position: absolute; top: -70px; right: -70px; width: 200px; height: 200px; border-radius: 50%; background: radial-gradient(circle, rgba(76, 155, 224, 0.55), transparent 70%); pointer-events: none; }
    .ref-card-label { position: relative; font-size: 12px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: #A9D2F5; }
    .ref-code { position: relative; font-size: 24px; font-weight: 800; letter-spacing: 0.08em; color: #FFFFFF; padding: 6px 14px; border-radius: 12px; border: 1.5px dashed rgba(255, 255, 255, 0.4); }
    .ref-card-hint { position: relative; font-size: 14px; font-weight: 600; color: #FFFFFF; }
    .ref-card-note { position: relative; font-size: 11px; color: rgba(255, 255, 255, 0.6); line-height: 1.4; max-width: 220px; }
    @media (max-width: 640px) {
      .ref-text { align-items: center; text-align: center; }
      .ref-text .title br { display: none; }
      .ref-offer { flex-direction: column; gap: 10px; }
      .ref-steps li { text-align: left; }
      .ref-share { display: grid; grid-template-columns: 1fr 1fr; width: 100%; }
      .share-btn { justify-content: center; }
    }
    @media (max-width: 340px) { .ref-share { grid-template-columns: 1fr; } .ref-card { min-width: 0; } }
  `,
})
export class Parrainage {
  private readonly document = inject(DOCUMENT);
  private readonly lien = `${this.document.location.origin}/devis`;
  private readonly message = `Je te recommande ABS clean pour tes vitres : devis gratuit en 2 minutes → ${this.lien}`;

  protected readonly copie = signal(false);
  protected readonly partageNatif = typeof navigator !== 'undefined' && typeof navigator.share === 'function';

  protected readonly lienWhatsApp = `https://wa.me/?text=${encodeURIComponent(this.message)}`;
  protected readonly lienSms = `sms:?&body=${encodeURIComponent(this.message)}`;
  protected readonly lienEmail = `mailto:?subject=${encodeURIComponent('Je te recommande ABS clean')}&body=${encodeURIComponent(this.message)}`;

  protected async copier(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.lien);
      this.copie.set(true);
    } catch {
      // Presse-papiers indisponible (contexte non sécurisé, refus) : le lien reste partageable par les autres boutons.
    }
  }

  protected async partager(): Promise<void> {
    try {
      await navigator.share({ title: 'ABS clean', text: this.message });
    } catch {
      // Partage annulé par l'utilisateur.
    }
  }
}
