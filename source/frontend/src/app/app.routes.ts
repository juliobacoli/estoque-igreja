import { Routes } from '@angular/router';

// Sem rotas no MVP de infraestrutura. O teste de deep-link (`/teste`) é do lado do
// servidor: recarregar nessa URL tem que devolver o index.html (MapFallbackToFile),
// e não 404. O AppComponent renderiza em qualquer URL.
export const routes: Routes = [];
