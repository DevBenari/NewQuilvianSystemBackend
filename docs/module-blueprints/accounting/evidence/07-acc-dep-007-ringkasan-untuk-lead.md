# `ACC-DEP-007` — Ringkasan untuk lead

| Field | Isi |
|---|---|
| Untuk | Lead / pemilik registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Dari | Rizki, owner modul Accounting |
| Tanggal | 2 September 2026 |
| Yang diminta | **Satu baris** ditambahkan ke registry di branch integration |
| Sifat | Read-only dari sisi Accounting. Nol perubahan dibuat pada tooling maupun registry oleh modul ini |

## Yang diminta, ringkas

Tambahkan satu baris ini ke
`NewQuilvianSystemBackend:docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` pada branch
`QuilvianIntegrationBackend`:

```
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |
```

Itu saja. Tidak ada perubahan tooling, tidak ada perubahan checker, tidak ada perubahan governance
lain yang diminta.

## Kenapa perlu

Registry ada dalam **dua salinan yang isinya berbeda**, dan checker QBE membaca yang tertinggal.

| Lokasi | Baris | `Acc` ada? |
|---|---:|---|
| `QuilvianEngineeringSkills` — registry canonical suite skill | 52 | **Ya, `ACTIVE`** sejak 1 September 2026 (`ACC-DEC-038`) |
| `NewQuilvianSystemBackend:docs/engineering/` @ `QuilvianIntegrationBackend` | 48 | **Tidak** |

Selisihnya **tepat empat baris, dan seluruhnya Accounting**. Ini bukan registry legacy dan bukan
berkas yang di-generate — ia `synced consumer` yang tertinggal satu pendaftaran.

## Akibatnya sekarang

Checker QBE **sudah hidup kembali** lewat PR #72 `b19c01e` pada 2 September 2026, yang memulihkan
path pembacaan governance ke `docs/engineering/`. Itu perbaikan yang benar dan sudah masuk.

Tetapi akibatnya berbalik arah:

| Sebelum PR #72 | Sesudah PR #72 |
|---|---|
| Checker mati — `TOOL ERROR: Canonical governance missing`, exit 2. Nol entity dievaluasi | Checker hidup, membaca registry 48 baris, dan **diperkirakan menolak `QBE-MOD-002 VIOLATION`** atas tujuh entity `Acc*` |

Jadi gerbang CI kini akan menolak merge Accounting justru karena checker-nya sudah benar — yang
salah tinggal isi registrinya.

## Yang sudah menumpuk menunggu merge

Per 2 September 2026, **tujuh task backend selesai** dan seluruhnya tertahan di pintu merge:

| Task | Isi |
|---|---|
| `BE-ACC-001` | Kerangka modul, 6 enum, test harness |
| `BE-ACC-002` | Audit hak akses badan hukum (read-only) |
| `BE-ACC-003`..`005` | **Tujuh entity persisted `Acc*`** — inilah yang akan ditolak checker |
| `BE-ACC-006` | Migration `20260902081432_AddAccountingFoundation` + seeder master |
| `BE-ACC-007` | API daftar akun, 8 endpoint |

Tumpukannya bertambah setiap task selesai. Owner modul memutuskan **terus berjalan tanpa menunggu**
(dicatat sebagai `ACC-TD-003` pada `UTANG-TEKNIS.md`), sehingga penambahan satu baris itu makin
lama makin menentukan.

## Riwayat yang perlu diketahui — dan satu koreksi

Akar `ACC-DEP-007` **bukan** `4db8909` seperti yang sempat tercatat. Urutan yang benar:

| Commit | Tanggal | Yang terjadi |
|---|---|---|
| `4db8909` | 28 Agt | Menghapus `agents/rules/engineering/` dari repo backend, checker tidak disesuaikan |
| `c9692d0` | 31 Agt | **PR #63 sudah memperbaikinya** — governance dipulihkan ke `docs/engineering/` **dan** checker diarahkan ke sana |
| `3d14cac` | 1 Sep | Merge **membatalkan bagian checker-nya**; berkas governance selamat |
| `b19c01e` | 2 Sep | PR #72 memperbaiki lagi bagian checker itu |

Buktinya kuat: blob checker `base=161ea88`, `ours=161ea88`, `theirs=3a9df9b`, hasil `161ea88`.
Karena `ours` identik dengan `base`, git otomatis mengambil `theirs` tanpa konflik — penimpaan
yang **tidak terlihat sebagai konflik**, bukan salah resolve.

Ini disebutkan bukan untuk menyalahkan siapa pun, melainkan karena pola yang sama akan terulang
pada pendaftaran prefix berikutnya bila kedua salinan registry tetap dibiarkan hidup terpisah.

## Usulan yang lebih besar, terserah lead

Selisih dua salinan registry akan lahir lagi setiap kali ada pendaftaran prefix baru. Dua jalan
yang mungkin, dan keduanya **milik lead**, bukan Accounting:

1. **Satu arah sinkronisasi yang tegas** — tetapkan suite skill sebagai satu-satunya sumber, dan
   jadikan salinan backend hasil generate, bukan berkas yang disunting tangan.
2. **Checker membaca sumber canonical langsung**, sehingga salinan backend tidak lagi diperlukan.

Accounting tidak mengusulkan yang mana; ia hanya mencatat bahwa butir ini akan berulang.

## Bukti pendukung

- `evidence/03-acc-dep-007-governance-propagation.md` — penelusuran lengkap
- `evidence/04-migration-coordination-gate.md` bagian 8 — temuan sampingan saat gate migration
- `UTANG-TEKNIS.md` butir `ACC-TD-003`

**Accounting tidak menambal checker maupun registry sendiri.** Keduanya milik lead, dan menambalnya
dari modul akan menyembunyikan sebabnya.
