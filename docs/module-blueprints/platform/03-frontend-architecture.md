# Arsitektur Frontend — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Blueprint | `PLT-BP-001` revisi 1 |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| Frontend SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` |
| Wewenang UI | **Belum ada brief UI yang disetujui.** Seluruh rupa navigasi `DEV_DISCRETION` |

---

## 1. Modul ini nyaris tidak punya frontend, dan itu benar

Dinyatakan di awal supaya tidak dicari-cari: **kemampuan inti slice ini tidak punya layar sama
sekali.** Alokasi nomor terjadi di dalam pekerjaan modul lain; petugas tidak pernah membukanya,
menekannya, atau memintanya.

Yang ada hanya **satu layar pemantauan berisi daftar baca-saja**, dan itu pun bukan untuk petugas
harian melainkan untuk administrator sistem yang sedang menelusuri keluhan.

Modul ini karena itu **tidak** menambah pekerjaan pada alur kerja siapa pun. Yang berubah bagi
petugas justru terjadi di modul lain: nomor order darah akhirnya bisa terbit.

---

## 2. Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Butir hak akses |
| --- | --- | --- | --- | --- | --- |
| Deret Nomor | 2 | Pengaturan Sistem | `/pengaturan/deret-nomor` | `PLT-FE-01` | `NumberSeries : Read` |

**Satu butir menu, satu layar.** Penempatannya di bawah rumpun pengaturan sistem adalah usulan,
bukan keputusan — belum ada brief navigasi yang disetujui. Yang **mengikat** hanya dua hal:
layarnya terjangkau dari menu, dan butir hak akses yang menjaganya adalah `NumberSeries : Read`.

Bila pemilik menghendaki layar ini tidak muncul di menu sama sekali dan hanya dijangkau lewat
tautan langsung, itu keputusan yang sah — asalkan dinyatakan, karena layar yang tidak terjangkau
dari mana pun dihitung belum selesai.

---

## 3. Skema fitur per layar

### `PLT-FE-01` — Daftar Deret Nomor

```text
┌──────────────────────────────────────────────────────────────┐
│  Deret Nomor                                                 │
├──────────────────────────────────────────────────────────────┤
│  [ Total deret: 3 ]  [ Total periode: 3 ]  [ Terakhir: … ]   │
├──────────────────────────────────────────────────────────────┤
│  Cari…            Kebijakan: [ semua ▾ ]          [ Reset ]  │
├──────────────────────────────────────────────────────────────┤
│  Penanda Deret     │ Periode │ Pengulangan │ Nilai │ Terakhir│
│  BBK_BLOOD_ORDER   │ GLOBAL  │ NEVER       │  123  │ 09:15   │
│  BBK_PROVIDER_REQ  │ GLOBAL  │ NEVER       │   47  │ 08:02   │
│  …                                                           │
├──────────────────────────────────────────────────────────────┤
│                                      ◄ 1 2 3 ►   25 / halaman│
└──────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong / gagal |
| --- | --- | --- | --- | --- |
| Kartu ringkasan | Jumlah deret, jumlah periode, waktu alokasi terakhir | `GET /summary` | `NumberSeries : Read` | Kosong → tampilkan nol, bukan galat. Belum ada deret adalah keadaan wajar sebelum nomor pertama terbit |
| Penyaring | Pencarian penanda deret, saringan kebijakan pengulangan | `GET /filters/metadata` | `NumberSeries : Read` | Gagal → pakai penyaring bawaan, jangan menghalangi daftar |
| Daftar | Penanda deret, periode, kebijakan, nilai sekarang, waktu terakhir | `GET /` | `NumberSeries : Read` | Kosong → "Belum ada deret nomor yang pernah dipakai." Gagal → tawarkan coba lagi |
| Detail | Satu deret pada satu periode | `GET /{id}` | `NumberSeries : Read` | Tidak ditemukan → kembali ke daftar dengan pesan |

**Nol tombol aksi di layar ini.** Tidak ada Tambah, Ubah, Hapus, maupun Setel Ulang — bukan karena
belum dibuat, melainkan karena backend memang tidak menyediakannya. Menyetel ulang pencacah
melanggar `INV-PLT-001`.

**Yang wajib terbaca jelas.** Kolom Nilai adalah nomor terakhir yang **sudah terbit**, bukan nomor
berikutnya. Perbedaan satu langkah ini menyesatkan bila labelnya kabur, jadi label kolom wajib
menyebutkannya.

---

## 4. Keadaan dan penanganannya

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka daftar, bukan layar kosong |
| Kosong | "Belum ada deret nomor yang pernah dipakai." Ini keadaan **wajar**, bukan galat |
| Gagal memuat | Pesan beserta tombol coba lagi |
| Tidak berhak | Butir menu tidak muncul; route langsung memulangkan penolakan dari backend |
| Data basi | Nilai pencacah bergerak terus. Layar **tidak** perlu memuat ulang otomatis — ia alat telusur, bukan pemantau waktu nyata. Sediakan tombol muat ulang manual |

---

## 5. Wewenang yang belum diturunkan

| Hal | Keadaan |
| --- | --- |
| Penempatan butir menu | **Usulan.** Belum ada brief navigasi yang disetujui |
| Rupa layar — warna, jarak, komponen | `DEV_DISCRETION`, mengikuti design system yang berlaku |
| Perlu-tidaknya layar ini di MVP | **Pertanyaan terbuka.** Lihat `04-prd-to-mvp.md`; slice ini tetap berguna penuh tanpa layar |
| Peringatan deret hampir habis | **Tidak dirancang** — `OQ-PLT-005` masih terbuka. Layar menampilkan nilai apa adanya tanpa ambang |
