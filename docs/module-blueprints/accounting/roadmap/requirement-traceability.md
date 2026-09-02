# Requirement Traceability — Accounting

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 4
roadmap_revision: 2
status: DRAFT_FORWARD_TEST
decision_revision: 1.1
backend_source: aa837d784ff51cb2b889cf975ada3a204018f1f5
frontend_source: fc49cc7714baa9a2c37ed6519fbaba5dffcbda99
contracts: [ACC-API-0.1, ACC-STATE-0.1, ACC-VALIDATION-0.2, ACC-INTEGRATION-0.2, ACC-PERMISSION-0.1, ACC-TEST-0.1, ACC-MVP-0.1]
```

Berkas ini menghubungkan requirement, keputusan, desain, task, dan bukti dalam satu tabel,
sehingga pertanyaan "aturan ini diwujudkan di mana, dan dibuktikan apa" dapat dijawab tanpa
membuka seluruh dokumen.

Status `Planned` berarti sudah terpetakan tetapi belum dikerjakan. **Belum ada satu pun yang
berstatus selesai**, karena belum ada implementasi.

---

## 1. Pemetaan requirement ke delivery

| Requirement / keputusan | Design / contract | Backend | Frontend | Bukti | Status |
|---|---|---|---|---|---|
| Daftar akun bertingkat per badan hukum (`ACC-DEC-022`, `ACC-DEC-037`) | `erd/01-chart-of-account.md`, `ACC-API-0.1` | `BE-ACC-003`, `BE-ACC-007` | `FE-ACC-002` | `FR-ACC-001`, `003`; `UAT-01` | Planned |
| Kode akun unik per badan hukum (`ACC-DEC-037`) | Unique index `(LegalEntityId, AccountCode)` | `BE-ACC-003`, `BE-ACC-007` | `FE-ACC-002` | `FR-ACC-002`; `UAT-15` | Planned |
| Kode akun terkunci setelah dipakai (`ACC-DEC-023`) | `ACC-VALIDATION-0.2` bagian 1 | `BE-ACC-007` | `FE-ACC-002` | `FR-ACC-005` | Planned |
| Akun bersaldo tidak dapat dinonaktifkan (`ACC-DEC-024`) | `ACC-VALIDATION-0.2` bagian 1 | `BE-ACC-007` | `FE-ACC-002` | `FR-ACC-004`; `UAT-17` | Planned |
| Alur jurnal berbeda per jenis (`ACC-DEC-010`) | `AccJournalType.RequiresApproval`, `ACC-STATE-0.1` | `BE-ACC-003`, `BE-ACC-008`, `BE-ACC-011` | `FE-ACC-003`, `FE-ACC-007` | `FR-ACC-006`, `007`, `030` | Planned |
| Kalender periode bulanan tahun kalender (`ACC-DEC-013`) | `erd/03-accounting-period.md` | `BE-ACC-004`, `BE-ACC-009` | `FE-ACC-004` | `FR-ACC-010` | Planned |
| Tiga status periode dan artinya (`ACC-DEC-012`) | `ACC-STATE-0.1` bagian 2.2 | `BE-ACC-004`, `BE-ACC-009`, `BE-ACC-011` | `FE-ACC-004` | `FR-ACC-012`, `013`; `UAT-06`, `UAT-07` | Planned |
| Hanya Manajer menutup periode (`ACC-DEC-026`) | `ACC-PERMISSION-0.1` | `BE-ACC-009` | `FE-ACC-004` | `FR-ACC-015` | Planned |
| Buka kembali wajib beralasan (`ACC-DEC-027`) | `ACC-VALIDATION-0.2` bagian 6 | `BE-ACC-009` | `FE-ACC-004` | `FR-ACC-015`; `UAT-09` | Planned |
| Setelah reopen hanya penyesuaian (`ACC-DEC-028`) | `ACC-STATE-0.1` bagian 2.1 | `BE-ACC-009` | `FE-ACC-004` | `FR-ACC-014`; `UAT-08` | Planned |
| Nomor jurnal boleh terlewat, tanpa penguncian (`ACC-DEC-014`) | `erd/02-journal.md` | `BE-ACC-010` | — | `FR-ACC-022`, `023`; `UAT-05` | Planned |
| Jurnal timpang boleh Draft (`ACC-DEC-025`) | `ACC-VALIDATION-0.2` bagian 3 dan 4 | `BE-ACC-010` | `FE-ACC-006` | `FR-ACC-020`, `021`; `UAT-02` | Planned |
| Keseimbangan diukur pada rupiah (`ACC-DEC-021`) | `ACC-VALIDATION-0.2` bagian 4, `NFR-008` | `BE-ACC-011` | `FE-ACC-006` | `FR-ACC-021`; `UAT-02` | Planned |
| Cost Center wajib pada akun beban (`ACC-DEC-019`) | `erd/02-journal.md`, reuse `MstCostCenter` | `BE-ACC-005`, `BE-ACC-010` | `FE-ACC-006` | `FR-ACC-024`; `UAT-04` | Planned |
| Rupiah saja, tanpa kurs (`ACC-DEC-020`) | `erd/data-dictionary.md` bagian 5 | `BE-ACC-005` | `FE-ACC-006` | Ketiadaan kolom pada kamus data | Planned |
| Satu jurnal satu badan hukum (`ACC-DEC-037`) | `erd/02-journal.md` | `BE-ACC-010` | `FE-ACC-001` | `FR-ACC-026`; `UAT-15` | Planned |
| Empat peran terpisah (`ACC-DEC-015`) | `ACC-PERMISSION-0.1` bagian 2 | `BE-ACC-011` | `FE-ACC-007` | `FR-ACC-030` | Planned |
| Pembuat tidak boleh menyetujui sendiri (`ACC-DEC-016`) | `ACC-PERMISSION-0.1` bagian 5 | `BE-ACC-011` | `FE-ACC-007` | `FR-ACC-031`; `UAT-03` | Planned |
| Riwayat yang disahkan permanen (`ACC-DEC-006`) | `ACC-STATE-0.1` bagian 1.2 | `BE-ACC-011` | `FE-ACC-007` | `FR-ACC-033`, `034`; `UAT-13` | Planned |
| Koreksi dua cara sesuai kasus (`ACC-DEC-017`) | `ACC-VALIDATION-0.2` bagian 5 | `BE-ACC-013` | `FE-ACC-010` | `FR-ACC-040`, `042`; `UAT-10`, `UAT-11` | Planned |
| Pembalikan perlu persetujuan baru (`ACC-DEC-029`) | `ACC-STATE-0.1` bagian 1.1 | `BE-ACC-013` | `FE-ACC-010` | `FR-ACC-043` | Planned |
| Buku besar dari jurnal disahkan saja | `02-backend-architecture.md` bagian 2 | `BE-ACC-012` | `FE-ACC-008` | `FR-ACC-051`, `052`; `UAT-14` | Planned |
| Laporan MVP: Neraca Saldo dan Buku Besar (`ACC-DEC-030`) | `ACC-API-0.1` grup General Ledger | `BE-ACC-012` | `FE-ACC-008`, `FE-ACC-009` | `FR-ACC-050`, `053`; `UAT-14`, `UAT-15` | Planned |
| Mulai dari saldo awal saja (`ACC-DEC-018`) | Saldo awal sebagai jurnal `SA` | `BE-ACC-014` | `FE-ACC-011` | `FR-ACC-060`; `UAT-16` | Planned |
| Saldo awal disahkan Manajer (`ACC-DEC-033`) | `ACC-PERMISSION-0.1` bagian 5 | `BE-ACC-014` | `FE-ACC-011` | `FR-ACC-061` | Planned |
| Enam peran final (`ACC-DEC-031`) | `ACC-PERMISSION-0.1` bagian 1 dan 2 | `BE-ACC-007` sampai `014` | `FE-ACC-007` | Matriks kewenangan | Planned |
| Pencatatan pembacaan dibatasi (`ACC-DEC-032`) | `ACC-PERMISSION-0.1` bagian 3 | `BE-ACC-012` | — | Pemeriksaan keluaran `LoggerService` | Planned |
| Reuse `MstCostCenter` dan `MstLegalEntity` | Tabel kepemilikan data | `BE-ACC-003`, `BE-ACC-005` | `FE-ACC-001` | `EV-ACC-002`, `EV-ACC-003` | **Terbukti** lewat evidence |
| Snapshot EF sudah pulih (`ACC-DEP-001`) | `02-backend-architecture.md` bagian 8 | `BE-ACC-006` | — | `EV-ACC-006` | **Terbukti** lewat evidence |

---

## 2. Pemetaan epic ke task

| Epic | Gelombang | Backend | Frontend | UAT |
|---|---|---|---|---|
| `EPIC ACC-01` Daftar akun | `MVP-0`, `MVP-1` | `BE-ACC-003`, `BE-ACC-007` | `FE-ACC-002` | `UAT-01`, `UAT-17` |
| `EPIC ACC-02` Jenis jurnal | `MVP-0`, `MVP-1` | `BE-ACC-003`, `BE-ACC-008` | `FE-ACC-003` | `UAT-01`, `UAT-18` |
| `EPIC ACC-03` Periode akuntansi | `MVP-0`, `MVP-1` | `BE-ACC-004`, `BE-ACC-009` | `FE-ACC-004` | `UAT-06` sampai `UAT-09` |
| `EPIC ACC-04` Jurnal manual | `MVP-0`, `MVP-1` | `BE-ACC-005`, `BE-ACC-010` | `FE-ACC-005`, `FE-ACC-006` | `UAT-01`, `UAT-02`, `UAT-04`, `UAT-05` |
| `EPIC ACC-05` Persetujuan dan pengesahan | `MVP-1` | `BE-ACC-011` | `FE-ACC-007` | `UAT-01`, `UAT-03`, `UAT-13` |
| `EPIC ACC-06` Koreksi dan pembalikan | `MVP-3` | `BE-ACC-013` | `FE-ACC-010` | `UAT-10`, `UAT-11`, `UAT-12` |
| `EPIC ACC-07` Buku besar dan neraca saldo | `MVP-2` | `BE-ACC-012` | `FE-ACC-008`, `FE-ACC-009` | `UAT-14`, `UAT-15` |
| `EPIC ACC-08` Saldo awal | `MVP-3` | `BE-ACC-014` | `FE-ACC-011` | `UAT-16`, `UAT-19` |

Empat task tidak terpetakan ke epic mana pun, dan itu disengaja:

| Task | Alasan |
|---|---|
| `BE-ACC-001` | Fondasi bersama seluruh epic — folder dan enum |
| `BE-ACC-002` | Audit read-only untuk menutup pertanyaan memblokir, bukan kemampuan bisnis |
| `BE-ACC-006` | Migration — melayani seluruh epic `MVP-0` sekaligus |
| `FE-ACC-001` | Kerangka bersama seluruh layar |

---

## 3. Coverage gap

Bagian ini menyebut keterkaitan requirement-ke-test yang **belum tertutup**. Menyembunyikannya
akan membuat Definition of Done terlihat lebih hijau daripada kenyataannya.

| Gap | Yang belum ada | Dampak | Tindakan |
|---|---|---|---|
| ~~`GAP-ACC-001`~~ | ~~`EPIC ACC-02` tanpa UAT gagal~~ | — | **DITUTUP** 1 September 2026 lewat `UAT-18` |
| ~~`GAP-ACC-002`~~ | ~~`EPIC ACC-08` tanpa UAT gagal~~ | — | **DITUTUP** 1 September 2026 lewat `UAT-19` |
| `GAP-ACC-003` | `ACC-DEC-032` pembatasan pencatatan pembacaan belum punya test otomatis | Pelanggaran hanya ketahuan lewat pemeriksaan manual keluaran logger | Tetapkan cara mengujinya saat `BE-ACC-012` dikerjakan, atau terima sebagai pemeriksaan manual dan catat |
| `GAP-ACC-004` | `NFR-002` penyimpanan bersamaan sulit diuji otomatis | `UAT-05` mengandalkan dua petugas menekan Simpan hampir bersamaan | **Ditautkan ke `BE-ACC-010`.** Ditutup lewat test integrasi konkurensi nyata yang menjalankan permintaan create paralel. `BE-ACC-010` **tidak boleh** `DONE` sebelum gap ini tertutup |
| `GAP-ACC-005` | ~~Cara pemberian hak atas `LegalEntityId` belum diketahui~~ → **caranya memang tidak ada** | Acceptance "badan hukum bukan hak pengguna ditolak `403`" **tidak dapat dirumuskan sama sekali** sampai mekanismenya dibuat | **Audit ditutup** 2 September 2026 oleh `BE-ACC-002`. Gap-nya sendiri **tetap terbuka** dan naik menjadi dependency `ACC-DEP-008`, milik owner keamanan platform |

Dua gap yang semula memblokir approval — `GAP-ACC-001` dan `GAP-ACC-002` — sudah **ditutup**
pada 1 September 2026 dengan menambahkan `UAT-18` dan `UAT-19`.

Tiga gap sisanya tidak memblokir approval maupun roadmap, tetapi masing-masing kini terikat pada
task tertentu sehingga tidak dapat menguap begitu saja.

| Gap | Task pengikat | Sifat ikatan |
|---|---|---|
| `GAP-ACC-003` | `BE-ACC-012` | Ditetapkan cara mengujinya, **atau** diterima sebagai pemeriksaan manual yang dicatat |
| `GAP-ACC-004` | `BE-ACC-010` | **Mengikat DoD.** `BE-ACC-010` tidak boleh `DONE` selama gap terbuka |
| `GAP-ACC-005` | ~~`BE-ACC-002`~~ → **`ACC-DEP-008`** | **Audit selesai, gap tidak tertutup.** `BE-ACC-002` menjawab pertanyaannya; jawabannya "mekanismenya tidak ada". Penutupan sesungguhnya menunggu Security/Platform menetapkan *Legal Entity Authorization Model*, dan menahan `BE-ACC-007` sampai `BE-ACC-014` |

---

## 4. Ketertelusuran keputusan yang ditunda

Sembilan pertanyaan `DEFERRED` pada `ACC-DEC-036` sengaja **tidak** muncul di tabel mana pun di
atas. Ini disengaja: memasukkannya akan membuat roadmap terlihat memuat pekerjaan yang sebenarnya
belum diputuskan.

| Pertanyaan ditunda | Kemampuan yang tertunda | Kapan dibuka kembali |
|---|---|---|
| `ACC-OQ-004`, `023`, `025`, `033`, `034` | Integrasi otomatis dan kotak masuk kejadian | Setelah `ACC-XM-001` diputuskan dan dua gerbang skill dilewati |
| `ACC-OQ-017` | Jurnal berulang | Perencanaan Phase 2 |
| `ACC-OQ-018`, `019`, `020` | Tutup buku berdaftar periksa | Perencanaan Phase 2 |

Penggantinya selama MVP berjalan tercatat di `04-prd-to-mvp.md` bagian 8.

---

## 5. Ringkasan kesiapan

| Ukuran | Nilai |
|---|---|
| Keputusan tertutup | 37 dari 37 |
| Kemampuan MVP terpetakan ke task | 9 dari 9 |
| Epic terpetakan ke task backend dan frontend | 8 dari 8 |
| Functional requirement terpetakan ke bukti | 30 dari 30 |
| Epic yang punya UAT berhasil **dan** gagal | **8 dari 8** |
| Task backend `READY` | 2 dari 14 |
| Task frontend `READY` | 0 dari 11 |
| Task yang sudah selesai | **0** |

Progres delivery **belum dapat dihitung sebagai persentase**, karena belum ada task yang
disetujui untuk dikerjakan. Denominatornya belum ada.
