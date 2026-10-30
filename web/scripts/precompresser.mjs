// Précompression du site compilé (lancée par le Dockerfile après « ng build ») :
// pour chaque script, feuille de style et SVG, une version Brotli (niveau maximal) et gzip à côté de l'original.
// L'API (Infrastructure/FichiersPrecompresses.cs) les sert telles quelles : meilleure compression, aucun calcul par visite.
import { readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { brotliCompressSync, constants, gzipSync } from 'node:zlib';

const racine = process.argv[2];
if (!racine) {
  console.error('Usage : node scripts/precompresser.mjs <dossier du site compilé>');
  process.exit(1);
}

let avant = 0;
let apres = 0;
for (const fichier of parcourir(racine)) {
  if (!/\.(js|css|svg)$/.test(fichier)) continue;
  const contenu = readFileSync(fichier);
  const br = brotliCompressSync(contenu, {
    params: { [constants.BROTLI_PARAM_QUALITY]: 11, [constants.BROTLI_PARAM_SIZE_HINT]: contenu.length },
  });
  const gz = gzipSync(contenu, { level: 9 });
  // Inutile de garder une version qui ne fait rien gagner (petits fichiers).
  if (br.length < contenu.length) writeFileSync(fichier + '.br', br);
  if (gz.length < contenu.length) writeFileSync(fichier + '.gz', gz);
  avant += contenu.length;
  apres += Math.min(br.length, contenu.length);
}
console.log(`Précompression : ${(avant / 1024).toFixed(0)} Ko → ${(apres / 1024).toFixed(0)} Ko (Brotli)`);

function* parcourir(dossier) {
  for (const nom of readdirSync(dossier)) {
    const chemin = join(dossier, nom);
    if (statSync(chemin).isDirectory()) yield* parcourir(chemin);
    else yield chemin;
  }
}
