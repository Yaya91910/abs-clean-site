import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-processus',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Processus</span>
          <h2 class="title">Comment ça marche</h2>
        </div>
        <ol class="grid-3">
          @for (etape of etapes; track etape.titre; let i = $index) {
            <li class="glass card">
              <span class="num" aria-hidden="true">{{ i + 1 }}</span>
              <h3>{{ etape.titre }}</h3>
              <p>{{ etape.description }}</p>
            </li>
          }
        </ol>
      </div>
    </section>
  `,
  styles: `
    ol { list-style: none; margin: 0; padding: 0; }
    .num { width: 44px; height: 44px; border-radius: 999px; background: linear-gradient(135deg, var(--accent2), var(--accent)); color: #FFFFFF; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 16px; }
    .card p { max-width: 250px; }
  `,
})
export class Processus {
  protected readonly etapes = [
    { titre: 'Remplissez le formulaire', description: 'Vitres, baies vitrées et velux, selon votre profil et la fréquence souhaitée.' },
    { titre: 'Recevez votre devis', description: "Le tarif s'affiche immédiatement à l'écran." },
    { titre: 'Prenez rendez-vous', description: 'Confirmez une date qui vous convient.' },
  ];
}
