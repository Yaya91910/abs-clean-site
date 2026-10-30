/**
 * Effets de la page d'accueil, sans dépendance :
 * - apparition en douceur des sections au défilement (celles déjà à l'écran au chargement s'affichent tout de suite) ;
 * - halos du fond en pause quand le haut de page n'est plus visible ;
 * - halo lumineux qui suit le pointeur sur les cartes « verre ».
 * Rien n'est activé si l'utilisateur a demandé à réduire les animations (prefers-reduced-motion).
 */
export function activerEffets(document: Document): void {
  const fenetre = document.defaultView;
  if (!fenetre || fenetre.matchMedia?.('(prefers-reduced-motion: reduce)').matches) return;

  if ('IntersectionObserver' in fenetre) {
    const observateur = new IntersectionObserver(
      (entrees) => {
        for (const entree of entrees) {
          if (!entree.isIntersecting) continue;
          entree.target.classList.add('vu');
          observateur.unobserve(entree.target);
        }
      },
      { rootMargin: '0px 0px -10% 0px' },
    );
    for (const section of Array.from(document.querySelectorAll('main > :not(abs-hero)'))) {
      if (section.getBoundingClientRect().top < fenetre.innerHeight) continue;
      section.classList.add('a-reveler');
      observateur.observe(section);
    }

    // Halos du fond (haut de page) : animation en pause une fois le héros sorti de l'écran.
    const page = document.querySelector('.page');
    const heros = document.querySelector('abs-hero');
    if (page && heros) {
      new IntersectionObserver(([entree]) => page.classList.toggle('calme', !entree.isIntersecting)).observe(heros);
    }
  }

  // Halo : position du pointeur dans la carte, reprise par le CSS (--mx, --my).
  document.addEventListener(
    'pointermove',
    (evenement) => {
      const carte = (evenement.target as Element | null)?.closest?.<HTMLElement>('.glass.card, .glass.price-card');
      if (!carte) return;
      const zone = carte.getBoundingClientRect();
      carte.style.setProperty('--mx', `${evenement.clientX - zone.left}px`);
      carte.style.setProperty('--my', `${evenement.clientY - zone.top}px`);
    },
    { passive: true },
  );
}
