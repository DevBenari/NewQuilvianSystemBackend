# Bukti Impact Scan — Re-baseline Source SHA Frontend

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` · Revision `3` (tidak naik) |
| Tanggal | 1 September 2026 |
| Pemicu | Frontend source SHA tidak cocok saat validasi status |
| SHA tercatat | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) |
| SHA nyata | `31a82c8052a3c59445ae49e6f1ccce2bf717d6c0` (branch `QuilvianIntegrationFrontend`) |
| Backend SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) — cocok, tidak terdampak |
| Sifat | Read-only terhadap source. Hanya artefak blueprint yang ditulis |
| Verdict | **Dampak rendah — nol artefak perlu direvisi** |

Aturan `/qv-lanjut` mewajibkan area terdampak dihentikan dan impact scan dijalankan lebih dahulu
ketika SHA berbeda. Berkas ini adalah hasil scan tersebut.

---

## `EV-ACC-006` — Sifat perpindahan: fast-forward, bukan divergensi

**Klaim yang diuji:** pindah dari `RizkiV2` ke `QuilvianIntegrationFrontend` tidak membuang
commit apa pun yang menjadi dasar blueprint.

```bash
git cat-file -t fc49cc7714baa9a2c37ed6519fbaba5dffcbda99
git branch -a --contains fc49cc7714baa9a2c37ed6519fbaba5dffcbda99
git rev-list --left-right --count RizkiV2...QuilvianIntegrationFrontend
```

**Hasil:**

- SHA tercatat masih hidup dan terjangkau dari **kedua** cabang, lokal maupun remote.
- `0	26` — `RizkiV2` punya **nol** commit yang tidak ada di `QuilvianIntegrationFrontend`.

**Kesimpulan:** `RizkiV2` terkandung penuh. Perpindahan baseline ini fast-forward murni; tidak
ada pekerjaan yang hilang dan tidak ada konflik yang perlu diselesaikan.

---

## `EV-ACC-007` — Isi drift: seluruhnya di luar wilayah Accounting

```bash
git log --oneline fc49cc7..HEAD
git diff --stat fc49cc7..HEAD
git diff --name-only fc49cc7..HEAD | awk -F/ '{print $1"/"$2"/"$3}' | sort | uniq -c | sort -rn
```

**Hasil:** 30 commit, 161 berkas, +24.636 / −87 baris. Sebarannya:

| Wilayah | Berkas |
|---|---|
| `src/components/view/` | 43 |
| `src/lib/hooks/` | 25 |
| `src/app/health-services/` | 22 |
| `src/lib/services/` | 16 |
| `src/lib/state/` | 14 |
| sisanya | utils, style, constants, tests |

Isinya: modul Operasi, Rekam Medis, Rawat Inap, dan kasir Billing — **seluruhnya di bawah
`health-services`**. Accounting berada di domain `Corporate`.

---

## `EV-ACC-008` — Enam anchor reuse yang dikutip blueprint frontend

**Klaim yang diuji:** pola reuse yang dikunci `03-frontend-architecture.md` masih berlaku.

```bash
git diff --name-only fc49cc7..HEAD -- <tiap berkas anchor>
```

| Anchor | Status |
|---|---|
| `src/components/features/TableModern/BaseTable.jsx` | tidak berubah |
| `src/components/features/base-features/data-table.jsx` | tidak berubah |
| `src/lib/state/slice/master-data-resource-slice-factory.jsx` | tidak berubah |
| `src/app/globals.css` | tidak berubah |
| `src/lib/state/store.jsx` | **berubah, aditif** |
| `src/utils/menu-sidebar/menu-items.jsx` | **berubah, aditif** |

**`store.jsx`** — 7 reducer Operasi ditambahkan; `masterDataDrugStorageLocation` hanya berpindah
posisi, masih terdaftar di baris 259. Diperiksa juga tidak ada duplicate key pada map reducer.
Pola registrasi slice yang direncanakan Accounting tetap sah.

**`menu-items.jsx`** — +75 baris, dua seksi baru: "Rekam Medis" dan "Operasi".

---

## `EV-ACC-009` — Belum ada kode Accounting yang bisa rusak

```bash
ls -d src/app/accounting src/app/corporate          # frontend
find . -type d -iname '*accounting*' -not -path './docs/*'   # backend
grep -rlE 'class Acc[A-Z]' --include=*.cs .                  # backend
```

**Hasil:** ketiganya kosong. `src/app/accounting/` dan `src/app/corporate/` tidak ada; backend
nol folder dan nol entity ber-prefix `Acc`.

**Kesimpulan:** delivery state `NOT_STARTED` akurat, dan permukaan tabrakan drift ini nol.

---

## Efek samping yang menguntungkan `ACC-FE-001`

`ACC-FE-001` (letak menu Accounting) masih terbuka dan menunggu product owner. Drift ini
menambah dua preseden segar di `menu-items.jsx` — "Rekam Medis" dan "Operasi" — yang dapat
dipakai sebagai contoh bentuk saat keputusan itu diambil. Ini **tidak** mengubah pertanyaannya,
hanya memperkaya bahan bandingannya.

---

## Yang tidak diperiksa

Scan ini menyasar dampak terhadap artefak Accounting, bukan kesehatan 161 berkas yang berubah.
Kebenaran modul Operasi, Rekam Medis, dan Rawat Inap berada di luar lingkupnya dan tidak dinilai.
