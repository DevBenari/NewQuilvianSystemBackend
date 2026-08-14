# Q-CARE — Dokumentasi Project Rules

| | |
|---|---|
| Tanggal | 2026-08-12 |
| Branch | `MHamzah` |
| Pemicu | Permintaan membuat dokumentasi aturan Q-CARE pada `docs/agency/` |
| Migration | Tidak ada |
| Breaking change | Tidak — dokumentasi saja |

## Kenapa diubah

Spesifikasi `Q-CARE_Developer_Handoff.pdf` memuat business rule, rekomendasi arsitektur,
contoh konfigurasi, dan keputusan yang masih terbuka dalam satu dokumen. Tanpa klasifikasi,
developer atau agent berisiko memperlakukan seluruh contoh sebagai aturan aktif, membuat
scheduler di frontend, menduplikasi entity milik modul lain, atau memberi Q-CARE kewenangan
menutup state IGD/billing yang bukan miliknya.

Dokumentasi baru memisahkan MUST/MUST NOT, CONFIG, SHOULD, dan DECISION REQUIRED serta
menetapkan ownership data, aturan backend/frontend, hubungan dengan IGD, Q-VOICE, dan
Clinical Charge Guard, minimum testing, Definition of Done, serta mekanisme aktivasi.

## Endpoint yang terpengaruh

Tidak ada. Tidak ada controller, DTO, service, entity, configuration, migration, atau
dependency injection yang diubah. Konvensi route dan candidate event di rulebook adalah
baseline desain yang masih memerlukan contract approval, bukan route/event yang diklaim
tersedia.

## Kontrak parameter / field

Tidak ada kontrak runtime yang berubah. Dokumentasi menetapkan format register yang wajib
untuk setiap rule yang telah diaktifkan, antara lain `RuleCode`, owner, source, version,
effective date, trigger, condition, schedule, stop condition, retry, escalation, override,
dan acceptance tests.

## File yang disentuh

| File | Perubahan |
|---|---|
| `docs/agency/q-care-project-rules.md` | Baru — calon rulebook canonical Q-CARE untuk project backend dan frontend |
| `docs/hamzah/report/q-care-project-rules.md` | Baru — laporan perubahan dokumentasi ini |

File lain yang sudah ada atau untracked di `docs/agency/`, `docs/Modul-RS/`, dan
`docs/Q-Care/` tidak diubah.

## Dampak ke frontend

Tidak ada source frontend yang berubah. Rulebook menetapkan bahwa frontend:

- tidak menjalankan scheduler/rules engine atau mengirim langsung ke provider;
- menggunakan API contract dan permission backend;
- tidak menampilkan UUID atau data sensitif;
- menangani duplicate submit, conflict, error, dan denied access;
- tetap memberi kewenangan menu, route, layout, dan tampilan kepada developer di bawah
  arahan atasan/product/UI lead.

## Cara menguji

Pemeriksaan dokumentasi:

1. pastikan metadata menyatakan status Proposed;
2. pastikan kategori MUST, MUST NOT, SHOULD, CONFIG, dan DECISION REQUIRED dijelaskan;
3. pastikan ownership dan daftar entity yang tidak boleh diduplikasi tersedia;
4. pastikan aturan backend, frontend, event, security, state, IGD, Q-VOICE, dan CCG tersedia;
5. pastikan open decisions serta mekanisme approval/activation tersedia;
6. pastikan tidak ada klaim bahwa endpoint, event, atau implementasi sudah aktif.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Struktur dan isi Markdown | **Lulus** — status Proposed, lima penanda normatif, ownership, backend/frontend, event, security, state, IGD, Q-VOICE/CCG, open decisions, dan aktivasi tersedia; code fence seimbang |
| Pemeriksaan Rule ID duplikat | **Lulus** — seluruh `QCARE-*` dan `OD-*` unik |
| Pemeriksaan perubahan file | **Lulus** — hanya dua dokumen pada tabel file yang disentuh dibuat oleh pekerjaan ini; source code tidak berubah |
| Review independen | **Lulus setelah revisi** — batas callback precedence, Q-VOICE, IGD, dan transactional outbox telah diperjelas |
| `dotnet build` | Tidak dijalankan — dokumentasi saja dan tidak ada source aplikasi yang berubah |
| Implementasi rule runtime | Belum — di luar scope pekerjaan dokumentasi |
