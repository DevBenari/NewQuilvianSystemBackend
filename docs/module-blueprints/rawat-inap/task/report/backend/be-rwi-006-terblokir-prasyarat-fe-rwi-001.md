# Laporan Task Backend — `BE-RWI-006` — **TERBLOKIR, TIDAK DIKERJAKAN**

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-006` |
| Judul | Status terisi dan dipesan hanya lahir dari modul Rawat Inap |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 dan bagian 5 |
| Branch | `MHamzah` |
| Commit backend saat pemeriksaan | `11711a1` |
| Tanggal pemeriksaan | 25 Agustus 2026 |
| Status | **BLOCKED — prasyarat lintas repository belum terpenuhi** |
| Berkas source yang diubah | **Tidak ada.** `BedController.cs` tidak disentuh sama sekali |

> Laporan ini ada supaya alasan tidak dikerjakannya task ini punya bukti tertulis, bukan hanya
> hidup di layar terminal orang yang memeriksanya. Ia **bukan** laporan perubahan, karena tidak
> ada satu baris source pun yang berubah.

---

## 1. Kenapa terblokir

Roadmap menyebut prasyaratnya di **tiga** tempat, dan ketiganya sepakat:

| Tempat | Bunyinya |
| --- | --- |
| Bagian 0 | "Yang **tetap belum** boleh: menerapkan migration ke database selain lokal, dan memulai `BE-RWI-006` sebelum `FE-RWI-001` terbukti rilis" |
| Bagian 4, baris **Dependency** | "`BE-RWI-004`; **dan `FE-RWI-001` pada roadmap frontend wajib sudah selesai**" |
| Bagian 4, baris **DoD** | "…`FE-RWI-001` terbukti sudah rilis…" |
| Bagian 5, tabel gerbang | "`FE-RWI-001` perbaikan tombol tempat tidur — Lintas repository — Menahan: `BE-RWI-006`" |

### 1.1 Bukti yang dicari, dan yang ditemukan

| Yang dicari | Hasilnya |
| --- | --- |
| Repository frontend `QuilvianSystemFrontendDev` di dalam workspace yang diberi wewenang | **Tidak ada.** Workspace hanya memuat `NewQuilvianSystemBackend` beserta dua folder skill |
| Laporan task frontend `task/report/frontend/fe-rwi-001-*.md` | **Tidak ada.** Folder `task/report/frontend/` sendiri belum pernah dibuat |
| Tanda selesai pada `roadmap/frontend-roadmap.md` untuk `FE-RWI-001` | **Tidak ada.** Task itu masih tanpa tanda, artinya belum dikerjakan |

Tidak ada satu pun bukti bahwa `FE-RWI-001` sudah rilis. Sesuai `AGENTS.md`, ketiadaan
repository frontend dilaporkan sebagai dependency yang hilang, **bukan** ditebak jalurnya.

---

## 2. Kenapa gerbang ini bukan formalitas

`BE-RWI-006` mencabut wewenang admin master data menyetel tempat tidur menjadi `Reserved` atau
`Occupied`. Admin tetap boleh menutup tempat tidur rusak lewat `Cleaning`, `Maintenance`,
`Blocked`, dan `Inactive` — **lewat tombol yang hari ini rusak**.

Tombol aktifkan dan nonaktifkan pada layar master tempat tidur memanggil endpoint `/activate`
dan `/deactivate` yang **tidak pernah ada**, dan selalu menerima 404 (`RWI-CON-TRC-001`,
`RWI-DEC-049`). `FE-RWI-001` memperbaikinya supaya keduanya memanggil `PATCH /beds/{id}/status`
yang memang ada.

> **Yang terjadi bila urutannya dibalik**, dikutip dari roadmap bagian 2:
>
> `BE-RWI-006` selesai hari Senin. Selasa pagi tempat tidur `MELATI-03-B` patah dan harus
> ditutup. Admin membuka layar master, menekan tombol nonaktifkan — dan menerima 404. Tempat
> tidur patah itu **tetap muncul** pada pencarian tempat tidur kosong, dan pasien berikutnya
> ditempatkan di sana.

Mengerjakan task ini lebih dulu berarti mencabut satu-satunya jalan keluar admin sebelum jalan
penggantinya berfungsi. Itu bukan risiko teknis; itu pasien yang ditempatkan di tempat tidur
rusak.

---

## 3. `BE-RWI-032` ikut tertahan

Roadmap menyatakan `BE-RWI-032` — test regresi modul tetangga — "dikerjakan bersamanya, bukan
sesudahnya", dan verifikasinya menuntut seluruh rangkaian test dijalankan **sebelum dan
sesudah** `BE-RWI-006`. Selama `BE-RWI-006` tertahan, `BE-RWI-032` ikut tertahan.

---

## 4. Yang harus terjadi supaya blokirnya terangkat

1. `FE-RWI-001` dikerjakan pada repository frontend dan **terbukti rilis**.
2. Laporannya ditulis di `docs/module-blueprints/rawat-inap/task/report/frontend/`, sesuai
   aturan lokasi laporan: laporan frontend tetap ditulis di repository backend, karena
   `docs/module-blueprints/` tinggal di sana.
3. Repository frontend disertakan pada workspace yang diberi wewenang, supaya buktinya dapat
   diperiksa dan bukan sekadar dinyatakan.
4. Baru setelah itu `BE-RWI-006` dan `BE-RWI-032` dijalankan bersama, dalam satu pengerjaan.

---

## 5. Catatan wewenang

Task ini adalah **satu-satunya** perubahan perilaku pada modul milik pihak lain di seluruh
roadmap ini. Persetujuan pemiliknya sudah ada lewat `RWI-DEC-062` untuk `MasterData`
HealthServices; yang belum terpenuhi murni urutan rilisnya, bukan persetujuannya.

`Areas/HealthServices/MasterData/Controllers/BedController.cs` **tidak disentuh** pada sesi ini.
