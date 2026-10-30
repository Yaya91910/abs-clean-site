import { ChangeDetectionStrategy, Component, DestroyRef, DOCUMENT, ElementRef, afterNextRender, inject, signal, viewChild } from '@angular/core';

/** Part de la vitre à essuyer avant qu'elle ne s'éclaircisse d'elle-même. */
const SEUIL_PROPRE = 0.55;

/**
 * Buée posée sur le visuel du héros : on l'essuie au doigt ou à la souris, comme une vraie vitre.
 * Un premier coup de raclette automatique montre le geste. Purement décoratif (aria-hidden),
 * absent si l'utilisateur a demandé à réduire les animations.
 */
@Component({
  selector: 'abs-buee',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true', '[class.active]': 'active()', '[class.propre]': 'propre()' },
  template: `
    <canvas #toile></canvas>
    <span class="indice" [class.cache]="touchee()">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none"><path d="M4 7h16M12 7v13" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" /></svg>
      Essuyez la vitre
    </span>
    <span class="bravo">✨ Impeccable !</span>
  `,
  styles: `
    :host { position: absolute; inset: 0; border-radius: inherit; overflow: hidden; display: block; pointer-events: none; touch-action: pan-y; }
    :host(.active:not(.propre)) { pointer-events: auto; }
    :host(:not(.active)) span { display: none; }
    canvas { position: absolute; inset: 0; width: 100%; height: 100%; cursor: grab; transition: opacity .9s ease; }
    :host(.propre) canvas { opacity: 0; }
    .indice, .bravo {
      position: absolute; left: 50%; top: 50%; translate: -50% -50%; display: inline-flex; align-items: center; gap: 8px;
      padding: 10px 16px; border-radius: 999px; background: rgba(255, 255, 255, 0.85); color: var(--ink);
      font-size: 14px; font-weight: 600; white-space: nowrap; pointer-events: none; box-shadow: 0 12px 30px -18px rgba(21, 52, 95, 0.5);
      transition: opacity .4s ease, scale .4s ease;
    }
    .indice.cache { opacity: 0; scale: .9; }
    .bravo { opacity: 0; scale: .8; }
    :host(.propre) .bravo { animation: bravo 2.4s ease forwards; }
    @keyframes bravo { 0% { opacity: 0; scale: .8; } 15%, 75% { opacity: 1; scale: 1; } 100% { opacity: 0; scale: 1; } }
  `,
})
export class Buee {
  private readonly toile = viewChild.required<ElementRef<HTMLCanvasElement>>('toile');
  private readonly hote = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly document = inject(DOCUMENT);

  protected readonly active = signal(false);
  protected readonly touchee = signal(false);
  protected readonly propre = signal(false);

  private ctx: CanvasRenderingContext2D | null = null;
  private dernier: { x: number; y: number } | null = null;
  private traits = 0;

  constructor() {
    const destruction = inject(DestroyRef);
    afterNextRender(() => {
      const fenetre = this.document.defaultView;
      if (!fenetre || fenetre.matchMedia?.('(prefers-reduced-motion: reduce)').matches) return;
      const canvas = this.toile().nativeElement;
      this.ctx = canvas.getContext('2d');
      if (!this.ctx) return;
      this.active.set(true);
      this.embuer();

      const bouger = (e: PointerEvent) => {
        // Souris : on essuie au survol ; doigt ou stylet : en glissant.
        if (e.pointerType !== 'mouse' && e.buttons === 0) return;
        this.essuyer(e.clientX, e.clientY);
      };
      const lever = () => (this.dernier = null);
      canvas.addEventListener('pointermove', bouger);
      canvas.addEventListener('pointerdown', bouger);
      canvas.addEventListener('pointerleave', lever);
      canvas.addEventListener('pointerup', lever);

      // Démonstration : un coup de raclette dès que la vitre est à l'écran, tant que personne n'y a touché.
      const observateur = new IntersectionObserver(([entree]) => {
        if (!entree.isIntersecting) return;
        observateur.disconnect();
        fenetre.setTimeout(() => this.demontrer(fenetre), 900);
      }, { threshold: 0.6 });
      observateur.observe(canvas);

      // Visuel redimensionné (rotation, police chargée…) avant qu'on y touche : nouvelle buée à la bonne taille.
      let taille = '';
      const redimension = new ResizeObserver(([entree]) => {
        const nouvelle = `${Math.round(entree.contentRect.width)}x${Math.round(entree.contentRect.height)}`;
        if (taille && nouvelle !== taille && this.traits === 0) this.embuer();
        taille = nouvelle;
      });
      redimension.observe(this.hote.nativeElement);
      destruction.onDestroy(() => {
        observateur.disconnect();
        redimension.disconnect();
      });
    });
  }

  /** Couche de buée : voile clair, grain de condensation et quelques gouttes. */
  private embuer(): void {
    const canvas = this.toile().nativeElement;
    const ctx = this.ctx!;
    const zone = this.hote.nativeElement.getBoundingClientRect();
    const ratio = Math.min(this.document.defaultView?.devicePixelRatio ?? 1, 2);
    canvas.width = Math.max(1, Math.round(zone.width * ratio));
    canvas.height = Math.max(1, Math.round(zone.height * ratio));
    ctx.setTransform(ratio, 0, 0, ratio, 0, 0);

    const fond = ctx.createLinearGradient(0, 0, zone.width, zone.height);
    fond.addColorStop(0, 'rgba(248, 250, 253, 0.97)');
    fond.addColorStop(1, 'rgba(234, 241, 249, 0.97)');
    ctx.globalCompositeOperation = 'source-over';
    ctx.fillStyle = fond;
    ctx.fillRect(0, 0, zone.width, zone.height);

    for (let i = 0; i < (zone.width * zone.height) / 60; i++) {
      ctx.fillStyle = `rgba(255, 255, 255, ${0.15 + Math.random() * 0.35})`;
      ctx.beginPath();
      ctx.arc(Math.random() * zone.width, Math.random() * zone.height, 0.4 + Math.random() * 1.3, 0, Math.PI * 2);
      ctx.fill();
    }
    for (let i = 0; i < 14; i++) {
      const x = Math.random() * zone.width, y = Math.random() * zone.height, r = 2 + Math.random() * 4;
      ctx.fillStyle = 'rgba(160, 186, 214, 0.45)';
      ctx.beginPath();
      ctx.arc(x, y, r, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
      ctx.beginPath();
      ctx.arc(x - r / 3, y - r / 3, r / 3, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  private essuyer(clientX: number, clientY: number): void {
    if (!this.ctx || this.propre()) return;
    const zone = this.toile().nativeElement.getBoundingClientRect();
    this.trait({ x: clientX - zone.left, y: clientY - zone.top }, Math.max(28, zone.width * 0.16));
  }

  private trait(point: { x: number; y: number }, largeur: number): void {
    const ctx = this.ctx!;
    this.touchee.set(true);
    ctx.globalCompositeOperation = 'destination-out';
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    const depart = this.dernier ?? point;
    // Bord humide (léger) puis cœur du trait (net), comme une raclette.
    for (const [epaisseur, force] of [[largeur * 1.25, 0.35], [largeur, 1]]) {
      ctx.lineWidth = epaisseur;
      ctx.strokeStyle = `rgba(0, 0, 0, ${force})`;
      ctx.beginPath();
      ctx.moveTo(depart.x, depart.y);
      ctx.lineTo(point.x + 0.01, point.y);
      ctx.stroke();
    }
    this.dernier = point;
    if (++this.traits % 12 === 0) this.verifier();
  }

  /** Coup de raclette en S, sur une seconde, pour montrer que la vitre s'essuie. */
  private demontrer(fenetre: Window): void {
    if (this.touchee() || !this.ctx) return;
    const zone = this.toile().nativeElement.getBoundingClientRect();
    const debut = fenetre.performance.now();
    const duree = 1100;
    const etape = (maintenant: number) => {
      const t = Math.min(1, (maintenant - debut) / duree);
      const doux = t < 0.5 ? 2 * t * t : 1 - (-2 * t + 2) ** 2 / 2;
      this.trait({ x: zone.width * (0.12 + 0.76 * doux), y: zone.height * (0.3 + 0.12 * Math.sin(doux * Math.PI * 2)) }, Math.max(24, zone.width * 0.13));
      if (t < 1) fenetre.requestAnimationFrame(etape);
      else {
        this.dernier = null;
        this.touchee.set(false);
      }
    };
    fenetre.requestAnimationFrame(etape);
  }

  /** Mesure la part essuyée (un pixel sur 64) ; au-delà du seuil, la vitre s'éclaircit entièrement. */
  private verifier(): void {
    const canvas = this.toile().nativeElement;
    const pixels = this.ctx!.getImageData(0, 0, canvas.width, canvas.height).data;
    let transparents = 0, total = 0;
    for (let i = 3; i < pixels.length; i += 4 * 64) {
      total++;
      if (pixels[i] < 40) transparents++;
    }
    if (total && transparents / total > SEUIL_PROPRE) this.propre.set(true);
  }
}
