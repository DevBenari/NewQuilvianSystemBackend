# Rubric Readiness Modul

Gunakan bobot yang relevan dengan modul dan tulis denominator. Jangan memaksakan bobot universal jika modul tidak memiliki suatu area.

## Dimensi minimum

| Dimensi | Pertanyaan utama |
| --- | --- |
| Foundation | Apakah ownership, model, kontrak, dan konfigurasi dasar tersedia? |
| Backend | Apakah rule, state transition, transaction, permission, DI, dan API berjalan? |
| Frontend | Apakah alur pengguna, state, permission, dan contract consumer selesai? |
| Integration/runtime | Apakah dependency, seed, worker/event, deployment config, dan failure path terbukti? |
| Verification | Apakah acceptance, contract, integration, regression, dan E2E memiliki bukti? |

## Format skor

| Dimensi | Bobot | Skor | Bukti utama | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Foundation | `<n>` | `<n>` | `<repo/path#symbol@sha>` | `<gap>` |

Laporkan dua angka jika relevan:

1. **Scaffold progress** — keberadaan struktur/schema/komponen dasar.
2. **End-to-end readiness** — alur yang dapat digunakan dengan rule dan runtime dependency terbukti.

## Aturan blocker

Terlepas dari skor agregat, verdict tidak boleh `READY` bila ditemukan:

- security/privacy invariant gagal;
- migrasi, DI, seed, atau konfigurasi wajib hilang;
- contract mismatch pada alur utama;
- state transition dapat melewati aturan penutupan;
- sumber kebenaran bersama diduplikasi secara konflik;
- acceptance scenario kritis tidak memiliki bukti.

