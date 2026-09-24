# Laporan Perubahan Frontend — `FE-ACC-P2-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-013` |
| Judul | Isian Jenis perlakuan pada form jenis kejadian |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-013` (revisi 5, `APPROVED`) |
| Trace | `ACC-DEC-087`; `02-backend-architecture.md` bagian 22.6. **Belum ada FR** — coverage gap yang sudah tercatat |
| Kontrak | `ACC-API-0.12` grup Event Type, bidang `EventKind` — dibangun `BE-ACC-P2-022`. Plus delta `AccountingEventCount` pada rincian (laporan `BE-ACC-P2-022` bagian 3.3) |
| Dependency | `BE-ACC-P2-022` ✅ — dikerjakan berurutan hari yang sama atas permintaan Rizki supaya backend dan layar diuji bersama |
| Task mode | `FRONTEND`; backend dibaca saja |
| Modul referensi visual | Layar Jenis Kejadian itu sendiri (`FE-ACC-P2-009`) — diperbarui, bukan dibuat ulang; isian pilihan enum meniru isian Perlakuan pada form Aturan Posting (`FE-ACC-P2-010`) |
| Branch | `RizkiV2` `c941012ac`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 24 September 2026 |
| Status | **🟡 SEBAGIAN** — 3 dari 3 acceptance di source; eslint 4 berkas **0 error, 0 warning**; uji layar Rizki 24 September 2026 lulus **7 dari 7** + 3 Swagger (bagian 9). **Satu-satunya yang belum:** `npm run build` owner (DoD) |

## 1. Yang dibangun

| Tempat | Perubahan |
| --- | --- |
| Form **Tambah** Jenis Kejadian | Isian pilihan baru **Jenis Perlakuan**: "Transaksi — dijurnal lewat aturan posting" (bawaan) atau "Saldo Subledger — disimpan sebagai saldo akun kontrol, tanpa jurnal" |
| Form **Perbarui** Jenis Kejadian | Isian yang sama, terisi dari data tersimpan. **Terkunci** bila jenis itu sudah dipakai kejadian, dengan keterangan "Terkunci — jenis ini sudah dipakai N kejadian, sehingga perlakuannya tidak dapat diubah." Kalimat pembuka form kini menyebut jenis perlakuan |
| Daftar Jenis Kejadian | Kolom baru **Jenis Perlakuan** di antara Modul Asal dan Aturan Posting Aktif |

**Dua lapis penguncian (acceptance 2).**

1. **Sebelum mencoba** — rincian dari backend membawa `accountingEventCount`. Bila lebih dari nol,
   isian langsung dimatikan saat form dibuka.
2. **Sesudah ditolak** — bila backend tetap menjawab `409` atas perlakuan yang diganti (mis. kejadian
   pertama tiba saat form terbuka, atau backend belum di-build sehingga angka itu belum dikirim),
   isian dikembalikan ke nilai tersimpan, dikunci, dan pesan backend tampil di toast apa adanya.

## 2. Kontrak yang dipakai

| Bidang | Arah | Nilai |
| --- | --- | --- |
| `eventKind` | Request `POST` dan `PUT` | Angka `1` (Transaksi) atau `2` (Saldo Subledger) — selalu dikirim, disalin dari `EventTypeKind.cs` |
| `eventKind` | Respons daftar dan rincian | Angka yang sama; nilai tak dikenal atau kosong ditampilkan "-" di daftar dan dibaca sebagai Transaksi di form |
| `accountingEventCount` | Respons rincian | Jumlah kejadian yang terhubung ke jenis itu |

## 3. `UI GATE`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Isian pilihan Jenis Perlakuan | `BaseEditorView` → `BaseEditorField` → `BaseSelectField` | `type: "select"` + `options` statis; nama `eventKind` tidak terdaftar di `SELECT_RESOURCES`, jadi tidak memanggil endpoint | REUSE | Deskriptor isian di constants |
| Penguncian beserta keterangannya | `BaseSelectField` props `disabled`, `description` | Pola yang sama dipakai isian Kode saat Perbarui | REUSE | Deskriptor isian diganti di hook saat terkunci |
| Kolom daftar | `DataTable` + render kolom yang ada di `event-type-view.jsx` | Satu cabang `format: "kind"` di samping `status`/`number` | REUSE | Teks biasa, bukan badge — perlakuan bukan status |
| Pesan `409` | `ToastStack` lewat `createToast` di hook | Jalur galat yang sudah ada | REUSE | — |

```
UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

Nol CSS baru; nol perubahan pada `globals.css` atau komponen global.

## 4. Pemetaan acceptance

| # | Acceptance | Bukti | Status |
| ---: | --- | --- | :---: |
| 1 | Bawaan Transaksi | `EVENT_TYPE_KIND_FIELD.defaultValue = "1"`; form Perbarui membaca nilai tersimpan, kosong → Transaksi (`readEventKind`). Diuji skenario 4 | ✅ |
| 2 | Isian terkunci saat ubah bila backend menjawab `409`, pesannya tampil | `eventKindLocked = isUpdate && (accountingEventCount > 0 \|\| kindRejected)`; `catch` pada `handleSubmit` menyetel `kindRejected` bila `statusCode === 409` dan perlakuan diganti, mengembalikan nilai, lalu toast pesan backend; `handleChange` menolak perubahan saat terkunci. Diuji skenario 2 (lapis pertama, terkunci sebelum mencoba); lapis kedua (sesudah `409`) dibuktikan lewat source — backend yang terbangun selalu mengirim `accountingEventCount`, sehingga lapis itu tidak dapat dipicu dari layar | ✅ |
| 3 | Angka yang dikirim cocok dengan enum backend | `EVENT_TYPE_KIND = { TRANSAKSI: 1, SALDO_SUBLEDGER: 2 }` = `EventTypeKind.cs` (`Transaksi = 1`, `SaldoSubledger = 2`); payload `eventKind: Number(form.eventKind)`; validasi form menolak selain 1/2. Diuji skenario 5–6: pilihan tersimpan dan terbaca kembali sama | ✅ |

## 5. Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/event-type/event-type-constants.jsx` | + `EVENT_TYPE_KIND`, `EVENT_TYPE_KIND_LABELS`, `EVENT_TYPE_KIND_OPTIONS`, `EVENT_TYPE_KIND_FIELD`; salinan `eventKindLocked`; kolom `eventKind`; isian di `createFields` dan `updateFields` |
| `src/lib/hooks/corporate/accounting/event-type/use-event-type-editor.jsx` | Validasi, pembacaan nilai tersimpan, penguncian dua lapis, `eventKind` di payload |
| `src/components/view/corporate/accounting/event-type/event-type-view.jsx` | Render kolom `format: "kind"` |
| `src/components/view/corporate/accounting/event-type/form/event-type-form-view.jsx` | Kalimat pembuka form Perbarui |

Nol baris komentar ditambahkan.

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` keempat berkas | **0 error, 0 warning** (exit 0) |
| Grep anti-regresi (`<button`, `<table`, `fw-`/`fs-`, warna literal, `style={{`) pada keempat berkas | Kosong semua |
| `npm run build` | **Belum dilaporkan** — Rizki. Uji layar berjalan di dev server, yang bukan bukti build produksi |
| `MANUAL TEST` | **PASS 7 dari 7** — Rizki, 24 September 2026, bagian 9 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — `test-policy.md`; `ACC-DEC-081` |

## 7. Skenario uji layar

Prasyarat: backend dengan `BE-ACC-P2-022` sudah di-build dan berjalan; `npm run dev`. Data yang
dipakai benar-benar ada: jenis **"Pembayaran Pasien"** (`PATIENT_PAYMENT`, modul `CASHIER`) yang
dipakai `EVT-UJI-002` (Terjurnal `JU/2026/09/00005`).

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| 1 | Menu Akuntansi › Jenis Kejadian | Kolom **Jenis Perlakuan** tampil; "Pembayaran Pasien" bertuliskan **Transaksi** |
| 2 | Klik dua kali "Pembayaran Pasien" | Form Perbarui; Jenis Perlakuan = Transaksi, **abu-abu/terkunci**, keterangan "Terkunci — jenis ini sudah dipakai 1 kejadian…" (angka sesuai data) |
| 3 | Di form yang sama, tanpa mengubah isian apa pun, tekan **Perbarui Jenis Kejadian** | Tersimpan; kembali ke daftar; perlakuan tetap Transaksi — isian yang terkunci tidak menghalangi simpan nama dan modul |
| 4 | **+ Tambah Jenis Kejadian** | Isian Jenis Perlakuan sudah terisi **Transaksi** |
| 5 | Isi Kode `UJI-SALDO-022`, Nama `Uji Saldo 022`, Modul `Finance`, pilih **Saldo Subledger**, simpan | Toast berhasil; di daftar baris baru bertuliskan **Saldo Subledger** |
| 6 | Buka `UJI-SALDO-022`, ganti ke **Transaksi**, simpan | Tersimpan (belum ada kejadian — isian tidak terkunci); daftar menampilkan Transaksi |
| 7 | Sesudah uji, tekan **Nonaktifkan** pada `UJI-SALDO-022` | Jenis uji nonaktif — jenis kejadian tidak dapat dihapus |

Skenario `409` lewat layar (lapis kedua) hanya muncul bila isian tidak terkunci padahal backend
menolak — misalnya backend lama tanpa `accountingEventCount`. Lapis itu dibuktikan lewat pembacaan
source; jalur `409` backend-nya diuji lewat Swagger pada laporan `BE-ACC-P2-022` skenario 5.

**Keterkaitan dengan `FE-ACC-P2-012` skenario 4.** Saat mendaftarkan jenis untuk `EVT-UJI-001`,
biarkan Jenis Perlakuan **Transaksi**. Bila dipilih Saldo Subledger, pesan berkode itu ditolak
`409` sampai `BE-ACC-P2-028` dibangun.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency backend | `BE-ACC-P2-022` ✅ |
| Risiko tersisa | Form Aturan Posting masih menawarkan jenis Saldo Subledger pada pilihan jenis kejadian — keputusan penyaringan terbuka, pemilik Rizki (laporan `BE-ACC-P2-022` bagian 7) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git frontend | Berkas task ini: 4 `M` di atas. Sisanya sudah ada sebelum task: `use-chart-of-account-editor.jsx`, `store.jsx`, `menu-items.jsx` (`M`) dan folder `accounting-event*` (`??`) milik `FE-ACC-P2-011`/`012` + perbaikan COA |
| Status Git backend | Berkas `BE-ACC-P2-022`: `EventTypeDtos.cs`, `AccEventTypeService.cs` (`M`), laporan `BE-ACC-P2-022.md` dan laporan ini (`??`), plus baris status di dua roadmap dan traceability. Sisanya milik `021`/`024`/`025` yang belum di-commit |
| Langkah berikutnya | Rizki: `npm run build` — satu build menutup `FE-ACC-P2-011`, `012`, dan `013` |

## 9. Hasil uji layar — Rizki, 24 September 2026

| # | Skenario | Hasil |
| ---: | --- | :---: |
| 1 | Kolom Jenis Perlakuan tampil; "Pembayaran Pasien" = Transaksi | ✅ |
| 2 | Form "Pembayaran Pasien": isian terkunci, keterangan "sudah dipakai N kejadian" | ✅ |
| 3 | Perbarui tanpa mengubah isian → tersimpan | ✅ |
| 4 | Tambah jenis → isian terisi Transaksi | ✅ |
| 5 | `UJI-SALDO-022` / Uji Saldo 022 / Finance / Saldo Subledger → tampil Saldo Subledger | ✅ |
| 6 | `UJI-SALDO-022` diganti ke Transaksi → berhasil | ✅ |
| 7 | `UJI-SALDO-022` dinonaktifkan | ✅ |
| Swagger | `PUT` "Pembayaran Pasien" `eventKind = 2` → `409`; `POST` `eventKind = 3` → `400`; `POST` tanpa `eventKind` → `201`, `eventKind = 1` | ✅ |

`MANUAL TEST: PASS` 7 dari 7, ditambah 3 uji Swagger milik `BE-ACC-P2-022`. UAT belum dijalankan —
diserahkan ke tim UAT.
