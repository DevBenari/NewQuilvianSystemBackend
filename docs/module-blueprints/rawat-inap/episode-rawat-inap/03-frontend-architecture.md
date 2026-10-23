# Rawat Inap — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | **`0.18`** — Peta Bangsal Denah, bagian 21, **approved_design_planned** 20 Oktober 2026 (`RWI-DEC-343` s.d. `353`; disetujui `RWI-DEC-352`). Riwayat: `0.17` Lencana Tier Membership & Prioritas Antrean Transfer (bagian 20); `0.16` Notifikasi Realtime Pemilihan Bed; `0.15` Admisi ODC; `0.14` Layar Transfer Pasien Masuk; `0.13` Integrasi Denah (`RWI-DEC-324`); `0.12` Bed Management; `0.11` Admisi Mba Ilma |
| Status | **`approved_design_planned`** untuk amandemen Peta Bangsal Denah (`0.18.0`, bagian 21; `RWI-DEC-352`, planning `PETA-DEL-20261020-01`); **`draft`** untuk amandemen Lencana Tier Membership & Prioritas Antrean Transfer (`0.17.0`, bagian 20); **`approved`** untuk amandemen Notifikasi Realtime (`0.16.0`), ODC (`0.15.0`), Transfer Pasien Masuk (`0.14.0`), dan Integrasi Denah (`0.13.0`) |
| Apa yang berubah pada `0.17` | **Lencana Tier Membership & Prioritas Antrean Transfer.** Tampilan antrean transfer menyajikan urutan CITO → Member Prioritas → Reguler. Tabel `InpatientAdmissionTransferList` dan langkah admisi `InpatientAdmissionTransferReviewStep` menampilkan lencana merah `[CITO]` dan lencana warna tier `[Platinum/Gold/Silver]`. Denah Bed Management bangsal umum bebas lencana tier demi privasi pasien (`RWI-DEC-341`). |
| Apa yang berubah pada `0.18` | **Peta Bangsal Denah.** Atas arahan atasan, denah Bed Management berganti rupa dari gambar arsitektur menjadi peta bangsal: kartu kamar di posisi asli, lempeng lantai, koridor, penanda, ringkasan lantai, dan titik bed. Rupa status kembali ke `RWI-DEC-303`/`316`. Aturan keras 16.7 butir 3 digantikan `RWI-DEC-343`. Nol perubahan backend dan API |
| Sub-modul | `episode-rawat-inap` — satu dari tiga sub-modul modul `rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082`. [Manifest sub-modul](./blueprint-manifest.md), [peta modul](../02-module-map.md) |
| Apa yang berubah pada `0.5` | **Hanya batas dokumen, bukan isi desain.** Peta butir menu seluruh modul naik ke [`../02-module-map.md`](../02-module-map.md) bagian 3, karena sidebar hanya satu untuk tiga sub-modul. Nol layar, endpoint, dan aturan keterjangkauan yang bergerak |
| Frontend SHA | Peta Bangsal: a491b05afcb6b3ed70218ccaa557007719d45ac3. Integrasi Denah: 4d9bf608dbbaddc96a2b4acfc8723753b5dd5028. Bed Management: 969acfcc04cdf31074a1911e9827c31d25ddadd0. Riwayat metadata: `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Backend SHA | Peta Bangsal: 6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd. Integrasi Denah: ca09cae909597442385ce4513be9c803549be10e. Bed Management: d4e1eca06fb28c05934c68c1e51a4dca01935a10. Riwayat metadata: `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Masukan | Peta Bangsal: decision 62, DEC-343–351, AC-498–507; gate 1.13/DNH-RCG-20261015-01; impact scan terfokus bagian 21.2. Integrasi Denah: decision 50, DEC-298–323, AC-436–461; gate 1.13/DNH-RCG-20261015-01; impact scan BM-IMP-20261015-01. Bed Management: Decision46, gate1.12/BM-RCG-20261010-01, audit BM-AUD-20261010-01 rev1. Riwayat metadata: `02-backend-architecture.md` revision `0.3`; `04-prd-to-mvp.md` revision `0.6.0` bagian 9; `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, dan `contracts/validation-matrix.md` revision `0.6.0`; `00-interview-decisions.md` revision `40` |
| Dasar revision ini | `RWI-DEC-075` s.d. `RWI-DEC-079`, `RWI-DEC-093` s.d. `RWI-DEC-096`, serta `RWI-DEC-267` s.d. `RWI-DEC-273` (10 Oktober 2026) |
| Batas tulis | Hanya dokumen blueprint |

> **Batas kewenangan dokumen ini.** Dokumen ini menetapkan **kontrak fungsional**: layar apa yang
> dibutuhkan, siapa boleh melakukan apa, data dan status apa yang dikonsumsi, urutan langkah pada
> alur yang berlangkah, dan bagaimana keadaan gagal ditangani.
>
> Dokumen ini **tidak** menetapkan warna, tata letak, pustaka komponen, nama menu, maupun nama
> route. Seluruhnya adalah wewenang pelaksana frontend selama tidak melanggar keamanan, privasi,
> invariant, atau **keterjangkauan** — lihat bagian 2B.

Urutan wewenang yang dipakai:

```text
keamanan / privasi / invariant / keterjangkauan
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

## 20. Amandemen frontend realtime pemilihan bed — `contract_version 0.16.0` (`approved_design_planned`, `RWI-DEC-337`)

### 20.1 Tujuan dan penempatan tanggung jawab

Koneksi realtime hidup selama `FloorPlanSelection` terpasang. Popup kamar bukan pemilik koneksi,
karena alert harus tetap diterima ketika pengguna sedang scroll atau popup tertutup. Event selalu
diikuti refetch; frontend tidak mengubah status bed hanya dari payload event.

### 20.2 Keputusan reuse atau buat baru

| Elemen | Keputusan | Alasan |
|---|---|---|
| `@microsoft/signalr` | Ready to reuse | Dependensi sudah tersedia |
| `createQueueRealtimeConnection` | Extend | Sudah menangani auth, automatic reconnect, dan rejoin; route hub sama |
| `useFloorPlan.refresh` | Ready to reuse | Jalur kanonis mengambil state dan eligibility terkini |
| Effect stale selection pada `FloorPlanSelection` | Ready to reuse | Otomatis memanggil `onClearSelection` sesudah hasil refresh tidak eligible |
| `useInpatientBedRealtime` | Baru | Mengisolasi lifecycle group, deduplikasi sesi, alert, reconnect, dan refetch |
| Alert pada `FloorPlanSelection` | Extend | Menjamin alert terlihat di seluruh konteks pemilihan, tidak hanya popup |
| Komponen alert visual | Reuse preferred | Gunakan base alert/toast Quilvian yang sudah ada; bentuk visual adalah `DEV_DISCRETION` |

### 20.3 Lifecycle hook

`useInpatientBedRealtime` menerima sekurangnya:

```text
enabled
currentUserId
serviceUnitIds       // distinct unit yang tampil dan lolos join server
onChanged(payload)  // set alert lalu await floorPlan.refresh()
```

Perilaku wajib:

1. Saat layar selection mount dan memiliki unit, start koneksi `/hubs/queues`, pasang handler
   `InpatientBedSelectionChanged`, lalu join tiap unit.
2. Saat daftar unit berubah, leave unit lama dan join unit baru; server tetap menjadi otoritas.
3. Abaikan event dengan `serviceUnitId` di luar unit aktif sebagai defense in depth.
4. Simpan `eventId` pada bounded in-memory set selama mount; event duplikat tidak membuat alert
   ganda. Set ini bukan history persisten.
5. Bila `actorUserId === currentUserId`, jangan tampilkan alert dan jangan mengumumkannya melalui
   accessibility live region. Refetch boleh tetap dilakukan.
6. Untuk actor lain, tampilkan alert segera, kemudian jalankan `state.refresh()`.
7. Setelah reconnect, rejoin semua unit aktif dan panggil `state.refresh()` segera; jangan meminta
   atau menampilkan replay alert lama.
8. Saat unmount, lepaskan handler, leave grup bila koneksi tersedia, dan hentikan instance koneksi
   milik hook.

### 20.4 Konten alert

| `eventType` | Pesan minimum |
|---|---|
| `BedReserved` | `Bed {bedLabel} telah didaftarkan atas nama pasien {patientName} oleh user {actorDisplayName}.` |
| `PatientPlaced` | `Bed {bedLabel} telah ditempati atas nama pasien {patientName} oleh user {actorDisplayName}.` |
| `PatientTransferred` | `Bed {bedLabel} telah menjadi tujuan transfer atas nama pasien {patientName} oleh user {actorDisplayName}.` |

`bedLabel` memakai `bedName` bila tersedia, lalu fallback `bedCode`. Alert harus dapat ditutup,
terbaca screen reader melalui live region yang sesuai, dan tidak memblokir scrolling. Warna, ikon,
posisi, durasi, dan animasi mengikuti design token/base component existing.

### 20.5 Refresh, pilihan usang, dan error

- Alert ditampilkan sebelum network refetch selesai agar pengguna segera memahami perubahan.
- Hasil refetch mengganti state denah. Effect existing membersihkan `selectedBedId` bila bed tidak
  lagi eligible.
- Bila refetch gagal, alert tetap terlihat dan error loading existing ditampilkan; frontend tidak
  boleh menyimpulkan status bed dari payload saja.
- Bila pengguna tetap submit sebelum event/refetch selesai, response conflict `409` existing tetap
  ditangani oleh hook admisi/transfer dan memaksa refresh.
- Kehilangan koneksi realtime tidak menonaktifkan aksi bisnis, karena server guard tetap menjamin
  satu pemenang.

### 20.6 Struktur file target

```text
QuilvianSystemFrontendDev/src/
├── lib/realtime/
│   └── queue-realtime-client.js                           [Extend event + group inpatient bed]
├── lib/hooks/health-services/inpatient-management/
│   └── use-inpatient-bed-realtime.js                      [Baru]
└── components/features/health-services/inpatient-management/floor-plan/
    └── floor-plan-selection.jsx                           [Extend hook + alert]
```

Tidak ada menu, route, halaman, atau perubahan navigation permission. Perubahan source frontend
yang sudah berada di working tree pengguna saat blueprint disusun tidak ditimpa dan bukan bukti
bahwa kontrak ini telah diimplementasikan.

---

## 0. Kenapa revision `0.4` ada

Revision `0.3` menghasilkan 18 task frontend yang seluruhnya selesai, tetapi hasilnya **tidak dapat
menjalankan alur bisnis yang PRD-nya sendiri tetapkan**. Sebabnya bukan mutu pelaksanaan, melainkan
tiga cacat pada dokumen ini:

| Cacat revision `0.3` | Akibat nyata di layar | Ditutup oleh |
| --- | --- | --- |
| Daftar layar tidak pernah diadu dengan `FLOW-RI-MVP-001` | Dua langkah alur yang PRD tandai **Wajib** tidak punya layar sama sekali: memilih penjamin dan memesan tempat tidur | bagian **2A** |
| Tidak ada model keterjangkauan; route dan menu diserahkan penuh ke `DEV_DISCRETION` | Beranda modul hanya berisi kalimat penantian; episode `Draft` dan `Closed` tidak dapat ditemukan dari layar mana pun; layar sesi koreksi jadi tetapi praktis mati | bagian **2B** |
| Aksi disebut pada matriks peran tetapi tidak dimiliki layar mana pun | "Memesan tempat tidur" dan "Membatalkan admisi" tidak pernah menjadi task | bagian **2A** dan **11A** |

Akibat gabungannya: **sembilan** operasi HTTP yang sudah jadi di backend tidak pernah dipanggil satu
pun layar. Daftarnya ada pada bagian 11A.

Revision `0.4` juga menyerap keputusan bentuk admisi. Admisi berhenti menjadi satu formulir dan
menjadi **alur berlangkah dua jalur** — pasien baru dan pasien lama — sesuai `RWI-DEC-075`.

### 0.1 Pertentangan di dalam revision `0.3` yang dibetulkan di sini

| Tempat | Isi lama | Betulnya |
| --- | --- | --- |
| bagian 2, `FE-INP-09` | "Tiga daftar pantau pada MVP" | **Empat** — konsisten dengan bagian 4.4 |
| bagian 9 | "Empat belas layar pada bagian 2" | Bagian 2 revision `0.3` memuat lima belas; revision ini memuat **sembilan belas** |
| bagian 2, `FE-INP-03` | "Membuka admisi, memilih penjamin, …" padahal `OpenAdmissionRequest` tidak punya kolom penjamin | Penjamin dipilih pada langkah **Pembayaran** dan ditulis lewat `POST /patient-encounters` milik Registrasi — lihat 3A |

---

## 1. Keadaan frontend saat ini

Delapan belas task revision `0.3` sudah dikerjakan, sehingga keadaan awal modul ini **bukan lagi nol**.

| Hal | Keadaannya |
| --- | --- |
| Route Rawat Inap | **13 route ada** di `src/app/health-services/inpatient-management/` |
| Menu Rawat Inap | **Ada**, delapan butir tingkat dua di `src/utils/menu-sidebar/menu-items.jsx:902` |
| Beranda modul | **Ada tetapi kosong** — hanya `Hero` dan satu kalimat penantian |
| Daftar kerja episode | **Tidak ada.** Census hanya memuat pasien yang sedang dirawat |
| Admisi | Satu formulir tunggal. Tanpa penjamin, tanpa pendaftaran pasien, tanpa pemesanan tempat tidur |
| Pemesanan tempat tidur | **Tidak ada layarnya.** Endpoint-nya ada dan menganggur |
| Pembatalan admisi | **Tidak ada layarnya.** Endpoint-nya ada dan menganggur |
| Cetak persetujuan dan kartu pasien | **Tidak ada** pada modul ini |
| Layar master tempat tidur | Tombol nonaktifkan sudah diperbaiki lewat `FE-RWI-001` |

Pola yang sudah terbukti berjalan di repository dan **wajib** dipakai ulang ada pada bagian 8.

---

## 2. Kebutuhan layar

Nama layar di bawah adalah **nama fungsional**, bukan nama menu. Pelaksana bebas menamai ulang dan
menggabungkan selama seluruh kemampuannya tercapai **dan** aturan keterjangkauan bagian 2B dipenuhi.

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-INP-01` | Daftar pasien dirawat (census) | Melihat siapa dirawat, di mana, oleh siapa, dan sudah berapa hari | Perawat, kepala ruangan, admisi, DPJP | ada |
| `FE-INP-02` | Papan ketersediaan tempat tidur | Melihat tempat tidur kosong, dipesan, terisi, ditutup — **dan mengonfirmasi pasien masuk** | Admisi, kepala ruangan, supervisor | ada, **bertambah aksi** |
| `FE-INP-03` | **Admisi pasien** | Alur berlangkah dua jalur: mendaftarkan atau menemukan pasien, menetapkan penjamin dan kelas, menetapkan unit dan DPJP, mencari lalu memesan tempat tidur, mencetak persetujuan dan kartu | Petugas admisi | **diganti total** |
| `FE-INP-04` | Detail episode | Melihat satu episode utuh: status, lokasi terkini, DPJP, perawat, riwayat | Semua peran klinis dan admisi | ada |
| `FE-INP-05` | Perpindahan pasien | Memindahkan pasien ke tempat tidur lain beserta alasannya | Kepala ruangan, perawat, supervisor, DPJP | ada |
| `FE-INP-06` | Keputusan pulang dan resume | DPJP menyatakan pasien boleh pulang lalu menyusun dan menandatangani resume | DPJP | ada |
| `FE-INP-07` | Penutupan episode | Menandai butir administrasi, melihat kelima syarat, lalu menutup episode | Petugas admisi, supervisor | ada |
| `FE-INP-08` | Penandaan kelayakan keuangan | Kasir menandai `Cleared` atau `Blocked` beserta catatannya | Kasir, billing | ada |
| `FE-INP-09` | Daftar pantau | **Empat** daftar pantau yang tersedia pada MVP | Admisi, kepala ruangan, supervisor | ada |
| `FE-INP-10` | Laporan selisih tempat tidur | Menemukan tempat tidur yang statusnya tidak cocok dengan penghuninya | Admin, supervisor | ada |
| `FE-INP-11` | Sesi koreksi episode | Supervisor membuka, mengoreksi, lalu menutup sesi | Supervisor | ada, **belum terjangkau** |
| `FE-INP-12` | Pengaturan Rawat Inap | Mengubah batas waktu dan ambang | Admin master data | ada |
| `FE-INP-13` | Master butir administrasi | Menambah, mengubah, dan menonaktifkan butir daftar periksa | Admin master data | ada |
| `FE-INP-14` | Pencatatan kepergian pasien | Menandai bahwa pasien sudah meninggalkan ruangan | Admisi, perawat, kepala ruangan, supervisor | ada |
| `FE-INP-15` | Penetapan kebutuhan isolasi | Merekam atau mengubah kebutuhan isolasi episode | Admisi selagi `Draft`; DPJP aktif setelah episode aktif | ada |
| `FE-INP-16` | **Daftar kerja episode** | Menemukan episode apa pun menurut status, unit, kelas, tanggal, dan kata kunci — termasuk `Draft` yang tertinggal dan `Closed` yang perlu dikoreksi | Semua peran ber-`InpatientEpisode : Read` | **baru** |
| `FE-INP-17` | **Pembatalan admisi** | Membatalkan admisi beserta pemesanan dan penempatannya dalam satu tindakan | Petugas admisi (`Draft`), kepala ruangan (`Admitted`), supervisor | **baru** |
| `FE-INP-18` | **Cetak persetujuan rawat inap** | Mencetak formulir persetujuan umum berisi data pasien, penjamin, dan episode | Petugas admisi | **baru** |
| `FE-INP-19` | **Beranda Rawat Inap** | Ringkasan operasional dan pintu masuk pekerjaan hari ini | Semua peran | **baru** |

Sembilan belas layar. `FE-INP-15` dan `FE-INP-17` **tidak wajib** berupa halaman tersendiri, tetapi
kedua jalur masuknya wajib ada — lihat 2B.

---

## 2A. Peta alur, layar, aksi, dan endpoint

**Bagian ini mengikat.** Setiap langkah `FLOW-RI-MVP-001` pada `04-prd-to-mvp.md` bagian 9 wajib
menunjuk tepat satu layar yang memilikinya. Langkah tanpa pemilik adalah **cacat blueprint**, bukan
`DEV_DISCRETION`, dan tidak boleh dibiarkan lewat seperti pada revision `0.3`.

| Langkah `FLOW-RI-MVP-001` | Layar pemilik | Endpoint |
| --- | --- | --- |
| 1. Memilih jenis kunjungan (Umum/Rujukan ringkas), kategori pasien, memilih pasien terdaftar (split layout kanan-kiri), atau mendaftarkannya (kontak darurat maks 13 digit) | `FE-INP-03` langkah 1–3 PB / langkah 1–2 PL | `GET /patients/options`, `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` |
| 2. Menentukan penjamin, kelas perawatan, dan deposit uang muka | `FE-INP-03` langkah 4–5 PB / langkah 3–4 PL | `POST /patient-insurances` atau `/patient-company-guarantors`, `GET …/deposit-policies` |
| 3. Menentukan DPJP & isolasi, sistem membuat kunjungan ranap (unit tujuan default tersembunyi) dan episode `Draft` | `FE-INP-03` **titik tulis 1** (langkah 6 PB / 5 PL) | `POST /patient-encounters` (`EncounterType=Inpatient`), `POST /episodes`, `PATCH …/isolation-requirement`, `POST …/top-ups` |
| 4. Mencari lalu **memesan** tempat tidur, `Reserved` selama 2 jam | `FE-INP-03` **titik tulis 2** (langkah 7–8 PB / 6–7 PL) | `GET /bed-occupancies/available-beds`, `POST /bed-occupancies/reservations` |
| 5. Konfirmasi admisi, lalu pengisian form persetujuan umum & tanda tangan digital pasien/wali (invariant bayi) | `FE-INP-03` **titik tulis 3 & 4** (langkah 9–10 PB / 8–9 PL) | `PUT /episodes/{id}`, `POST /episodes/{id}/admission-consents` |
| 6. Mencetak lembar persetujuan rawat inap ber-TTD digital dan kartu pasien | `FE-INP-18` (langkah 11 PB / 10 PL) & `FE-INP-03` langkah 12 PB | Dialog cetak peramban / printer kartu |
| 7. Pasien sampai di kamar; **konfirmasi masuk**; kelayakan diperiksa ulang | **`FE-INP-02`** | `POST /bed-occupancies/placements` |
| 8. Menugaskan perawat penanggung jawab | `FE-INP-04` | `POST /episodes/{id}/nurse-assignments` |
| 9. DPJP mengubah kebutuhan isolasi di tengah perawatan | `FE-INP-15` di dalam `FE-INP-04` | `PATCH /episodes/{id}/isolation-requirement` |
| 10. Memindahkan pasien | `FE-INP-05` | `POST /bed-occupancies/placements/transfer` |
| 11. Mengalihkan DPJP | `FE-INP-04` | `POST /episodes/{id}/doctor-assignments` |
| 12. Menyatakan pasien boleh pulang | `FE-INP-06` | `POST /discharges/{episodeId}/decide` |
| 13. Menyusun dan menandatangani resume | `FE-INP-06` | `PUT /discharges/{episodeId}/summary`, `PATCH …/summary/sign` |
| 14. Menandai butir administrasi | `FE-INP-07` | `POST /discharges/{episodeId}/clearance/{itemId}/mark` |
| 15. Menandai kelayakan keuangan | `FE-INP-08` | `POST /discharges/{episodeId}/financial-clearance` |
| 16. Mencatat kepergian pasien | `FE-INP-14` | `POST /discharges/{episodeId}/record-departure` |
| 17. Menutup episode | `FE-INP-07` | `POST /discharges/{episodeId}/close` |
| 18. Menutup menembus gerbang keuangan | `FE-INP-07` | `POST /discharges/{episodeId}/close-with-override` |
| 19. Sesi koreksi | `FE-INP-11` | `POST /episodes/{id}/correction-sessions`, `PATCH …/close` |

### Aksi di luar alur utama, yang tetap wajib punya pemilik

| Aksi | Layar pemilik | Endpoint |
| --- | --- | --- |
| Menemukan episode apa pun menurut status | `FE-INP-16` | `GET /episodes`, `GET /episodes/filters/metadata` |
| Melanjutkan admisi `Draft` yang tertinggal | `FE-INP-16` → `FE-INP-03` | `GET /episodes/{id}`, `PUT /episodes/{id}` |
| Membatalkan admisi | `FE-INP-17` | `PATCH /episodes/{id}/cancel` |
| Membatalkan pemesanan sebelum dipakai | `FE-INP-03` langkah 8 PB / 7 PL dan `FE-INP-02` | `PATCH /bed-occupancies/reservations/{id}/cancel` |
| Ringkasan operasional | `FE-INP-19` | `GET /episodes/summary`, `GET /census/summary` |
| Mencetak persetujuan rawat inap ber-TTD digital | `FE-INP-18` dan Workspace PPRI | cetak dari data persetujuan & TTD digital yang sudah disimpan |
| Mencetak kartu pasien baru | `FE-INP-03` langkah 12 PB | dipakai ulang dari cetak kartu kiosk |

---

## 2B. Arsitektur informasi dan keterjangkauan

> **Pindah tempat 2026-09-02 — `RWI-DEC-082`.** Sidebar hanya satu untuk seluruh modul, sedangkan
> modul ini kini punya tiga sub-modul. Peta butir menu **seluruh modul** karena itu naik ke
> [`../02-module-map.md`](../02-module-map.md) bagian 3, supaya tiga sub-modul tidak merancang
> sidebar sendiri-sendiri.
>
> Yang tinggal di bawah ini adalah **aturan keterjangkauan** dan **peta route** milik sub-modul
> `episode-rawat-inap`. Kesembilan butir menu tingkat dua yang dihitung `IA-INP-05` seluruhnya
> milik sub-modul ini — **kuotanya sudah penuh**. Karena itu `keperawatan` dan `dokter-rawat-inap`
> tidak dapat menambah butir menu tingkat dua tanpa lebih dulu mengubah `IA-INP-05`, dan hasil
> penetapannya ditulis kembali ke `02-module-map.md`, bukan ke berkas ini.

**Nama route, nama menu, dan urutan menu tetap `DEV_DISCRETION`. Keterjangkauan tidak.**

### Aturan yang mengikat

| Aturan | Isinya |
| --- | --- |
| `IA-INP-01` | Setiap layar pada bagian 2 wajib dapat dicapai dari Beranda `FE-INP-19` dalam **paling banyak tiga klik** |
| `IA-INP-02` | Setiap layar per-episode wajib punya **paling sedikit satu** jalur masuk dari `FE-INP-16` |
| `IA-INP-03` | `FE-INP-16` wajib dapat menampilkan **kelima** nilai status episode, termasuk `Draft`, `Cancelled`, dan `Closed`. Census **tidak boleh** dipakai sebagai penggantinya, karena census berarti "sedang dirawat" dan mencampurnya mengaburkan arti itu |
| `IA-INP-04` | Layar yang tidak terjangkau dari mana pun dihitung **belum selesai**, walaupun kodenya ada dan test-nya lulus |
| `IA-INP-05` | Menu tingkat dua Rawat Inap dibatasi **paling banyak sembilan** butir. Layar per-episode tidak mendapat butir menu; ia dicapai lewat `FE-INP-16` atau `FE-INP-01` |

### Peta route usulan

Mengikat pada kolom kanan, bebas pada kolom kiri.

| Route usulan | Yang wajib terjangkau dari sana |
| --- | --- |
| `/health-services/inpatient-management` | `FE-INP-19` — ringkasan, dan tautan ke seluruh layar tingkat dua |
| `…/admissions` | `FE-INP-03` seluruh langkah, termasuk melanjutkan `Draft` lewat parameter episode |
| `…/episodes` | `FE-INP-16` |
| `…/episodes/{id}` | `FE-INP-04`, dan dari sana `FE-INP-05`, `FE-INP-14`, `FE-INP-15`, `FE-INP-17` |
| `…/episodes/{id}/discharge` | `FE-INP-06` |
| `…/episodes/{id}/financial-clearance` | `FE-INP-08` |
| `…/episodes/{id}/closure` | `FE-INP-07` |
| `…/episodes/{id}/correction` | `FE-INP-11` |
| `…/episodes/{id}/consent-print` | `FE-INP-18` |
| `…/bed-board` | `FE-INP-02` termasuk konfirmasi masuk |
| `…/census` | `FE-INP-01` |
| `…/monitoring`, `…/bed-drift` | `FE-INP-09`, `FE-INP-10` |
| `…/settings`, `…/clearance-items` | `FE-INP-12`, `FE-INP-13` |

### Isi Beranda `FE-INP-19`

Yang **wajib** terbaca, bentuknya bebas:

1. Jumlah pasien dirawat per unit layanan dan per kelas — dari `GET /census/summary`.
2. Jumlah episode per status — dari `GET /episodes/summary`. Angka `Draft` adalah **penanda kerja
   tertinggal** dan wajib dapat diklik menuju `FE-INP-16` yang sudah tersaring `Draft`.
3. Jumlah baris pada keempat daftar pantau, masing-masing dapat diklik.

Beranda **tidak boleh** menjadi halaman yang hanya berisi judul dan kalimat penantian.

---

## 2C. Kontrak komposisi layar

| Bentuk | Dipakai untuk | Aturannya |
| --- | --- | --- |
| **Daftar** | `FE-INP-01`, `FE-INP-09`, `FE-INP-10`, `FE-INP-13`, `FE-INP-16` | Wajib memenuhi keempat keadaan bagian 5.1 |
| **Papan** | `FE-INP-02` | Wajib memuat ulang saat difokuskan kembali dan sebelum dialog konfirmasi |
| **Alur berlangkah** | `FE-INP-03` | Kontraknya ada pada bagian 3A |
| **Ruang kerja per episode** | `FE-INP-04` | Menjadi induk `FE-INP-05`, `FE-INP-14`, `FE-INP-15`, `FE-INP-17` |
| **Layar anak per episode** | `FE-INP-06`, `FE-INP-07`, `FE-INP-08`, `FE-INP-11`, `FE-INP-18` | Wajib membaca ulang detail episode saat dibuka |
| **Cetak** | `FE-INP-18` dan kartu pasien | Halaman cetak sendiri, tanpa navigasi aplikasi |

Aturan tambahan: **satu kemampuan, satu tempat.** Bila sebuah aksi sudah dimiliki satu layar, layar
lain hanya boleh menautkannya, tidak boleh menyediakan jalur keduanya. Pelajaran ini datang dari
koreksi resume yang sengaja hanya ada di `FE-INP-11`.

---

## 3. Aksi per peran

Kolom ini menurunkan langsung dari `contracts/permission-audit-matrix.md` revision `0.4.0`. Tombol
yang tidak diizinkan **harus disembunyikan atau dinonaktifkan**, bukan ditampilkan lalu ditolak
server.

| Aksi di layar | Petugas admisi | Perawat | Kepala ruangan | DPJP | Kasir | Supervisor | Admin |
| --- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Melihat census dan daftar kerja | Ya | Ya | Ya | Ya | Ya | Ya | – |
| Mencari tempat tidur kosong | Ya | Ya | Ya | – | – | Ya | – |
| Mendaftarkan pasien baru | Ya | – | – | – | – | Ya | – |
| Membuka admisi | Ya | – | – | – | – | Ya | – |
| **Memesan tempat tidur** | Ya | – | – | – | – | Ya | – |
| **Membatalkan pemesanan** | Ya | – | – | – | – | Ya | – |
| **Mengonfirmasi pasien masuk (penempatan)** | Ya | – | – | – | – | Ya | – |
| Membatalkan admisi `Draft` | Ya | – | – | – | – | Ya | – |
| Membatalkan admisi `Admitted` | – | – | Ya | – | – | Ya | – |
| Memindahkan pasien | – | Ya | Ya | Ya, bila DPJP aktif | – | Ya | – |
| Mengalihkan DPJP | – | – | Ya | – | – | Ya | – |
| Menugaskan perawat | – | – | Ya | – | – | Ya | – |
| Menyatakan pasien boleh pulang | – | – | – | Ya, bila DPJP aktif | – | – | – |
| Menyusun dan menandatangani resume | – | – | – | Ya, bila DPJP aktif | – | – | – |
| Menandai butir administrasi | Ya | – | – | – | – | Ya | – |
| Menandai kelayakan keuangan | – | – | – | – | Ya | – | – |
| Menutup episode | Ya | – | – | – | – | Ya | – |
| Mencatat pasien sudah meninggalkan ruangan | Ya | Ya | Ya | – | – | Ya | – |
| Menutup menembus gerbang keuangan | – | – | – | – | – | Ya | – |
| Membuka sesi koreksi | – | – | – | – | – | Ya | – |
| Menetapkan kebutuhan isolasi | Ya, hanya selagi `Draft` | – | – | Ya, bila DPJP aktif | – | – | – |
| **Mencetak persetujuan rawat inap** | Ya | – | Ya | – | – | Ya | – |
| Mengubah pengaturan dan butir | – | – | – | – | – | – | Ya |

### 3.1 Tiga tombol yang paling perlu diperhatikan

| Tombol | Aturan tampilnya |
| --- | --- |
| Pindahkan pasien, untuk pengguna berperan dokter | Hanya aktif bila dokter itu **DPJP aktif episode tersebut**. Bila bukan, tombol dinonaktifkan disertai keterangan "Anda bukan DPJP episode ini" |
| Tutup menembus gerbang keuangan | **Tidak boleh** ditampilkan berdampingan dengan tombol tutup biasa seolah dua pilihan setara. Ia baru muncul setelah tombol tutup biasa ditolak karena kelayakan keuangan, dan hanya untuk supervisor |
| Ubah kebutuhan isolasi | Kewenangannya **berpindah** mengikuti status episode. Selagi `Draft`, aktif bagi petugas admisi dan bagi DPJP. Begitu `Admitted`, tombol itu **nonaktif** bagi petugas admisi, disertai keterangan "Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP". Bagi dokter yang bukan DPJP aktif, keterangannya "Anda bukan DPJP episode ini" |

### 3.2 Konfirmasi masuk — satu butir hak akses yang perlu diputuskan pemilik

`RWI-DEC-076` memisahkan penempatan dari alur admisi: alur admisi berhenti pada tempat tidur
`Reserved`, dan pasien baru menjadi `Admitted` ketika kedatangannya dikonfirmasi.

Kalimat usulan yang disetujui menyebut **perawat ruangan** sebagai pelakunya. Kontrak hak akses
revision `0.4.0` **tidak mengizinkannya**: `POST /bed-occupancies/placements` menuntut
`InpatientBedOccupancy : Create`, dan butir itu hanya dimiliki **petugas admisi** dan **supervisor**.
Perawat pelaksana serta kepala ruangan hanya memiliki `Read` dan `Transfer`.

| Yang diberlakukan revision ini | Alasannya |
| --- | --- |
| Konfirmasi masuk dijalankan **petugas admisi** dan **supervisor** dari `FE-INP-02` | Dokumen frontend tidak berwenang melonggarkan kontrak hak akses yang sudah terkunci. Melonggarkannya diam-diam akan menghasilkan tombol yang pasti ditolak server — cacat yang sama seperti tombol isolasi pada revision `0.3` |

**Butir terbuka `RWI-OQ-045`:** apakah `InpatientBedOccupancy : Create` perlu diberikan kepada
kepala ruangan supaya konfirmasi masuk dapat dilakukan dari ruangan. Kalau ya, perubahannya ada di
`contracts/permission-audit-matrix.md` dan seeder hak akses backend — **di luar** dokumen ini.
Owner: Product/Domain bersama Backend/API.

---

## 3A. Kontrak alur admisi berlangkah — `FE-INP-03`

Dasar: `RWI-DEC-075` sampai `RWI-DEC-079`, `RWI-DEC-093` sampai `RWI-DEC-096`, serta amandemen revisi Mba Ilma `RWI-DEC-267` sampai `RWI-DEC-273`.

### 3A.1 Dua jalur masuk

Layar dibuka dengan pilihan tipe pendaftaran (`patient-entry-choice-step.jsx`):

| Jalur | Kapan dipakai | Jumlah langkah |
| --- | --- | :---: |
| **Pendaftaran pasien baru** | Pasien belum pernah terdaftar di rumah sakit | **12 langkah** |
| **Pendaftaran pasien lama** | Pasien sudah memiliki nomor rekam medis | **10 langkah** |
| **Admisi Kamar Pulih** | Pasien rujukan dari instalasi bedah/kamar pulih | 10 langkah (lihat `FE-INP-29`) |

Keduanya bermuara pada langkah yang selaras sejak langkah Pembayaran.

### 3A.2 Langkah jalur pasien baru (12 Langkah)

Menyerap revisi tim analisis bisnis (`RWI-DEC-267` s.d. `RWI-DEC-273`):

| # | Langkah | Isi yang wajib ada | Titik tulis | Endpoint |
| :---: | --- | --- | :---: | --- |
| 1 | **Jenis Kunjungan** | Pilihan `Umum` vs `Rujukan`. Bila `Umum`, langsung lanjut. Bila `Rujukan`, wajib mengisi data rujukan tekstual ringkas: nomor rujukan, tanggal/jam, faskes perujuk, dokter perujuk, dan diagnosa awal rujukan (`RWI-DEC-269`, `RWI-AC-390`). Tanpa kewajiban upload dokumen fisik | – | – (state alur) |
| 2 | **Kategori Pasien** | Tepat 3 opsi: `Pasien Umum`, `Bayi Baru Lahir`, dan `Pegawai` (`RWI-DEC-270`, `RWI-AC-391`). Opsi Ibu, Anak, dan Korporat dinonaktifkan. Bila `Bayi Baru Lahir`, pemilih episode ibu aktif dan mengisi `MotherEpisodeId` | – | `GET /episodes?status=Admitted` |
| 3 | **Pendaftaran** | Scan KTP (opsional), form identitas pasien baru, dokumen identitas, dan kontak darurat. **Nomor HP Kontak Darurat dibatasi maksimal 13 digit angka** (`RWI-DEC-267`, `RWI-AC-388`) | **tulis pasien** | `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` |
| 4 | **Pembayaran** | Cara bayar: Tunai, Asuransi, atau Penjamin Perusahaan. Pendaftaran/pemilihan kartu penjamin. **Kelas perawatan dipilih di sini** | tulis kartu baru bila ada | `POST /patient-insurances` atau `POST /patient-company-guarantors` |
| 5 | **Deposit** | Nominal uang muka/deposit, perbandingan terhadap minimum kebijakan. Nominal kurang hanya memperingatkan, tidak menolak (`RWI-DEC-095`) | – (ditahan di klien) | `GET …/patient-funds/deposit-policies` |
| 6 | **Dokter** | **DPJP aktif**, catatan admisi, sakelar dan keterangan isolasi. **Dropdown Unit Tujuan disembunyikan dari UI** (`RWI-DEC-268`, `RWI-AC-389`) dan otomatis mengikat default Service Unit Rawat Inap pada backend payload | **titik tulis 1** | `POST /patient-encounters` → `POST /episodes` → `PATCH …/isolation-requirement` → `POST …/top-ups` (deposit bila ada) |
| 7 | **Pilih Bed** | Hasil `available-beds` yang tersaring server menurut kelas dan isolasi | – | `GET /bed-occupancies/available-beds` |
| 8 | **Booking Bed** | Memesan tempat tidur terpilih (`Reserved` 2 jam). Sisa waktu wajib terbaca | **titik tulis 2** | `POST /bed-occupancies/reservations`, `PATCH …/reservations/{id}/cancel` |
| 9 | **Konfirmasi** | Ringkasan admisi. Menyimpan perubahan catatan/kelas bila ada | **titik tulis 3** | `PUT /episodes/{id}` |
| 10 | **Form Persetujuan & TTD Digital** | **Langkah Mandiri (*Dedicated Step*)**: Form isian persetujuan umum rawat inap, pilihan penanda tangan (Pasien / Wali), data wali. **Invariant: Bayi Baru Lahir mengunci penanda tangan ke Orang Tua/Wali** (`RWI-DEC-273`). Kanvas tanda tangan digital interaktif (`react-signature-canvas`). Data persetujuan & citra TTD tersimpan permanen ke episode (`RWI-DEC-272`, `RWI-AC-393`) | **titik tulis 4** | `POST /episodes/{id}/admission-consents` |
| 11 | **Cetak Persetujuan Ranap** | Formulir persetujuan terisi lengkap dengan citra TTD digital siap cetak (`FE-INP-18`). Staf PPRI hanya melakukan print tanpa perlu input ulang (`RWI-AC-395`) | – | Dialog cetak peramban |
| 12 | **Kartu Pasien** | Cetak kartu berobat pasien baru | – | dipakai ulang dari cetak kartu kiosk |

### 3A.3 Langkah jalur pasien lama (10 Langkah)

Menyerap penyatuan pencarian-verifikasi dan penataan rujukan (`RWI-DEC-271` dan `RWI-AC-392`):

| # | Langkah | Isi | Titik tulis | Endpoint |
| :---: | --- | --- | :---: | --- |
| 1 | **Cari & Verifikasi Pasien Terpadu** | **Layar Split 1 Langkah**: Kanan memuat form pencarian Nomor RM/NIK + tombol Cari; Kiri memuat kartu identitas hasil verifikasi data pasien lengkap (`RWI-DEC-271`). Tombol Lanjut aktif setelah 1 pasien diverifikasi | – | `GET /patients/options?search=...` |
| 2 | **Jenis Kunjungan & Kategori Pasien** | Pilihan `Umum` vs `Rujukan` (form rujukan ringkas teks) DAN Kategori Pasien (`Umum` atau `Pegawai`; opsi Bayi Baru Lahir dinonaktifkan pada pasien lama) | – | – (state alur) |
| 3 | **Pembayaran** | Cara bayar, kartu penjamin, dan kelas perawatan | tulis kartu bila baru | sama dengan Langkah 4 PB |
| 4 | **Deposit** | Uang muka / deposit rawat inap | – | sama dengan Langkah 5 PB |
| 5 | **Dokter** | DPJP & isolasi; Unit Tujuan disembunyikan dari UI dan diisi default ranap | **titik tulis 1** | sama dengan Langkah 6 PB |
| 6 | **Pilih Bed** | Pemilihan tempat tidur yang tersedia | – | sama dengan Langkah 7 PB |
| 7 | **Booking Bed** | Reservasi tempat tidur aktif | **titik tulis 2** | sama dengan Langkah 8 PB |
| 8 | **Konfirmasi** | Kunci admisi dan ringkasan | **titik tulis 3** | sama dengan Langkah 9 PB |
| 9 | **Form Persetujuan & TTD Digital** | Form persetujuan umum rawat inap & kanvas digital signature | **titik tulis 4** | sama dengan Langkah 10 PB |
| 10 | **Cetak Persetujuan Ranap** | Pratinjau dokumen ber-TTD siap cetak. Tombol utama berbunyi **Selesai Admisi** dan menutup alur | – | sama dengan Langkah 11 PB |

Langkah **Kartu Pasien tidak ada** pada jalur pasien lama; pasien lama sudah memilikinya.

### 3A.4 Empat titik tulis alur admisi

`RWI-DEC-076` dan `RWI-DEC-272` menetapkan tulisan terjadi bertahap untuk menjamin integritas data:

| Titik tulis | Terjadi di | Yang tersimpan |
| :---: | --- | --- |
| pasien | akhir langkah Pendaftaran (PB) | `MstPatient` beserta dokumen identitas dan kontak darurat (No HP maks 13 digit) |
| **1** | akhir langkah Dokter | Kunjungan rawat inap (`TrxPatientEncounter`), episode `Draft` (`InpEpisode`), catatan isolasi, dan top-up deposit awal |
| **2** | langkah Booking Bed | `InpBedReservation` aktif; tempat tidur berstatus `Reserved` selama 2 jam |
| **3** | langkah Konfirmasi | Pembaruan catatan episode dan kelas perawatan bila ada koreksi |
| **4** | langkah Form Persetujuan & TTD Digital | Formulir persetujuan umum rawat inap, data subjek/wali penanda tangan, waktu tanda tangan, dan citra tanda tangan digital (`base64 PNG`) |

### 3A.5 Aturan mundur

| Mundur dari | Ke | Yang boleh berubah |
| --- | --- | --- |
| Booking Bed | Pilih Bed | Bebas, selama pemesanan yang sudah terbentuk dibatalkan terlebih dahulu |
| Pilih Bed | Dokter | Catatan dapat diubah lewat `PUT /episodes/{id}`. **DPJP tidak dapat diubah** dari alur admisi; admisi dibatalkan bila DPJP salah |
| Dokter | Deposit / Pembayaran | **Tidak boleh** setelah Titik Tulis 1 terlewati. Kunjungan, episode, dan deposit awal sudah terbentuk. Pembatalan admisi diperlukan jika salah penjamin |
| Form Persetujuan | Konfirmasi | Bebas. Catatan yang ditinjau masih dapat disesuaikan |
| Cetak Persetujuan | Form Persetujuan | Bebas. Kanvas tanda tangan dapat dibersihkan dan ditandatangani ulang selama belum dicetak/difinalisasi |

### 3A.6 Alur yang ditinggal

| Ditinggal setelah | Yang tertinggal di server | Cara menemukannya kembali |
| --- | --- | --- |
| Langkah Pendaftaran | Pasien terdaftar tanpa kunjungan | Jalur pasien lama |
| Titik tulis 1 | Episode `Draft` tanpa tempat tidur | `FE-INP-16` tersaring `Draft` → melanjutkan alur ke langkah Pilih Bed |
| Titik tulis 2 | Episode `Draft` dengan tempat tidur `Reserved` | Sama; sisa waktu pemesanan wajib terbaca |
| Titik tulis 4 | Episode `Draft` lengkap dengan persetujuan dan TTD | `FE-INP-16` → langsung ke langkah Cetak Persetujuan atau konfirmasi kedatangan di Bed Board |

### 3A.7 Batas alur ini

| Yang **tidak** dilakukan alur admisi | Alasannya |
| --- | --- |
| Menempatkan pasien menjadi `Admitted` | `RWI-DEC-076`. Pasien baru menjadi `Admitted` saat kedatangannya dikonfirmasi di Papan Tempat Tidur |
| Mengalihkan DPJP di tengah jalan | Menjadi wewenang detail episode `FE-INP-04` melalui `doctor-assignments` |
| Memaksa upload dokumen rujukan fisik | `RWI-DEC-269`. Pendaftaran mencatat ringkasan teks rujukan tanpa membebani pemindaian berkas |
| Menagih input ulang di Workspace PPRI | `RWI-DEC-272` dan `RWI-AC-395`. Dokumen persetujuan yang sudah ditandatangani di admisi langsung tersedia *read-only / print-ready* di PPRI |

### 3A.8 Persetujuan rawat inap & TTD Digital — Hubungan dengan Workspace PPRI

Menyerap `RWI-DEC-272`, `RWI-DEC-273`, dan `RWI-AC-393` s.d. `RWI-AC-395`:

| Aspek | Ketetapannya |
| --- | --- |
| Formulir terstruktur | Formulir persetujuan memuat identitas pasien/wali, penjamin, nomor episode, pernyataan persetujuan tindakan, pelepasan informasi, tata tertib, dan penunjukan penerima informasi medis |
| Tanda tangan digital | Pasien atau wali menandatangani langsung di layar menggunakan stylus / touch / mouse via `react-signature-canvas`. Citra disimpan dan dilekatkan pada berkas |
| Invariant Bayi Baru Lahir | Untuk pasien kategori Bayi Baru Lahir, sistem mengunci penanda tangan secara mutlak ke Orang Tua / Wali sah (`RWI-DEC-273`) |
| Hubungan dengan Workspace PPRI | Berkas yang tersimpan di langkah admisi ini langsung mengisi menu General Consent di Workspace PPRI. Menu PPRI berstatus **read-only / print-ready**; staf PPRI dapat mencetak berkas kapan saja tanpa perlu meminta pasien menandatangani ulang (`RWI-AC-395`) |

---

## 4. Data dan status yang dikonsumsi

### 4.0 Sumber data lintas modul

Setiap isian pilihan wajib punya satu sumber yang disebut namanya. Isian tanpa sumber yang tertulis
adalah cara paling mudah sebuah kemampuan hilang tanpa disadari.

| Isian | Pemilik data | Sumber |
| --- | --- | --- |
| Pasien | PatientManagement | `patients/options`, atau pendaftaran baru pada alur admisi |
| Penjamin dan cara bayar | PatientManagement dan RegistrationManagement | `patient-insurances`, `patient-company-guarantors`, `insurance-providers`, `company-guarantors` |
| Dokter (DPJP) | MasterData HealthServices | isian pilihan sumber daya yang sudah ada |
| Perawat | HR / MasterData | isian pilihan sumber daya yang sudah ada |
| Unit layanan | MasterData HealthServices | isian pilihan sumber daya; **wajib bertipe `Inpatient`** |
| Kelas perawatan | MasterData HealthServices | isian pilihan sumber daya |
| Kamar dan tempat tidur | MasterData HealthServices | **hanya** lewat `available-beds` dan `bed-board`, tidak pernah dari master langsung — lihat 4.3A |
| Butir administrasi | Rawat Inap | `inpatient-clearance-items` |
| Pengaturan | Rawat Inap | `inpatient-settings` |

### 4.1 Status episode dan cara menampilkannya

| Nilai dari backend | Kata yang diusulkan | Yang wajib terbaca pengguna |
| --- | --- | --- |
| `Draft` | Admisi sedang disiapkan | Pasien belum tentu ada di kamar. **Bila tempat tidur sudah dipesan, sisa waktu pemesanan wajib terbaca** |
| `Admitted` | Sedang dirawat | Pasien menempati tempat tidur |
| `DischargePending`, kepergian belum dicatat | Rencana pulang | Sudah boleh pulang, episode belum ditutup, tempat tidur **masih dipegang** |
| `DischargePending`, kepergian sudah dicatat | Sudah pulang, menunggu penutupan | Pasien sudah tidak di ruangan dan tempat tidur **sudah bebas**, tetapi episodenya belum ditutup |
| `Closed` | Selesai | Episode ditutup, tempat tidur sudah dilepas |
| `Cancelled` | Batal | Admisi tidak jadi berjalan |

Kata pada kolom kedua adalah **usulan**. Yang mengikat hanya kolom ketiga.

### 4.2 Status tempat tidur

| Nilai | Kata yang diusulkan | Catatan penting |
| --- | --- | --- |
| `Available` | Tersedia | — |
| `Reserved` | Dipesan | **Wajib menampilkan sisa waktu pemesanan** dan, pada layar yang berhak, episode yang memegangnya |
| `Occupied` | Terisi | Wajib menampilkan nama pasien pada layar yang berhak |
| `Cleaning`, `Maintenance`, `Blocked` | Pembersihan, Perbaikan, Diblokir | Disetel admin, bukan oleh Rawat Inap |
| `Inactive` | Nonaktif | — |

**Jebakan penamaan yang wajib dihindari.** Backend punya dua hal berbeda yang sama-sama bernama
"status": `IsActive` pada tempat tidur, dan `BedStatus`. Layar **tidak boleh** memakai kata "status"
sendirian. Pakai "keadaan tempat tidur" untuk `BedStatus` dan "aktif/nonaktif" untuk `IsActive`.
Dasarnya `RWI-CON-TRC-002`.

### 4.3 Lama dirawat — `RWI-FE-001`, `DEV_DISCRETION`

`RWI-RULE-019` menghitung lama dirawat dari **selisih tanggal**, bukan selisih jam, dengan hasil
paling sedikit 1 hari.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Angka itu wajib terbaca jelas sebagai **hitungan hari rawat**, bukan lama waktu sebenarnya |
| Yang **bebas** | Bentuk kalimat, singkatan, penempatan, dan gaya tampilan |
| Kenapa penting | Pasien masuk 21 Sept pukul 22:30 dan pulang 22 Sept pukul 06:00 tercatat **1 hari**, padahal hanya 7,5 jam |

### 4.3A Kelayakan penempatan pada papan tempat tidur, pemesanan, dan perpindahan

| Aspek | Ketetapannya |
| --- | --- |
| Hasil `GET /available-beds` | Sudah tersaring server memakai aturan Kelayakan Penempatan. Layar **tidak boleh** menyaring ulang sendiri |
| Tempat tidur yang tersaring keluar | **Boleh** ditampilkan sebagai baris nonaktif disertai alasannya, dan ini dianjurkan |
| Pemesanan | Memakai daftar dan penyaring yang sama. Tambahannya: tempat tidur ber-`IsReservable` salah ditolak dengan pesan "Tempat tidur ini tidak dapat dipesan." |
| Konfirmasi masuk | Kelayakan diperiksa **ulang** di sini. Penolakan pada tahap ini **wajar** dan wajib terbaca sebagai keadaan yang berubah, bukan sebagai kesalahan petugas |
| Perpindahan | Aturan yang sama persis |
| ~~Kamar yang terhalang pencampuran~~ | ~~Pesan penolakan dari server **menyebut nama kamarnya** dan ditampilkan apa adanya~~ **DICABUT 11 September 2026** oleh `RWI-DEC-101`. Keadaan ini tidak pernah terjadi lagi |

#### 4.3A.1 Pencabutan `ROOM_GENDER_MIXED` — pekerjaan lintas repository

Server tidak pernah lagi menerbitkan kode `ROOM_GENDER_MIXED`. Yang tersisa di frontend adalah
pemetaan pesan untuk kode yang sudah mati, ditambah test yang menguncinya.

| Yang wajib dibereskan di frontend | Letaknya |
| --- | --- |
| Hapus entri `ROOM_GENDER_MIXED` dari pemetaan kode penolakan | `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` |
| Hapus atau ganti tiga assertion yang menguncinya | `tests/unit/inpatient-placement.test.mjs` |
| Ganti skenario e2e yang mengharapkan penolakan itu | `tests/e2e/inpatient-episode-detail.spec.mjs` |
| Sesuaikan pesan `PATIENT_GENDER_UNKNOWN` | Kalimat "di kamar yang belum ada penghuninya" dicabut; pesan baru ada pada `contracts/validation-matrix.md` bagian 3 |

**Urutannya mengikat.** Bila backend turun lebih dulu, test frontend gagal karena menguji kode yang
tidak pernah terbit. Bila frontend turun lebih dulu, petugas menerima penolakan tanpa pesan yang
dapat dibaca. Karena itu keduanya **wajib satu gelombang rilis**, dan itu sudah dicatat pada
`contracts/api-contract.md` `0.8.0`.

**Satu hal yang tidak boleh ikut terhapus.** `isIsolationFailure` pada berkas yang sama menguji
kode isolasi, dan salah satu assertion-nya kebetulan memakai `ROOM_GENDER_MIXED` sebagai contoh
kode yang **bukan** kegagalan isolasi. Assertion itu perlu contoh pengganti, bukan dihapus, supaya
pembedaan isolasi tetap teruji.

### 4.4 Bentuk daftar pantau — `RWI-FE-002`, `DEV_DISCRETION`

**Empat** daftar pantau tersedia pada MVP: penutupan tertunda, penutupan menembus gerbang keuangan,
episode tanpa perawat penanggung jawab, dan penempatan tidak sesuai kebutuhan isolasi. Daftar pantau
kepatuhan pengkajian dan CPPT **belum ada** karena bergantung pada slice yang menunggu `DEC-INP-001`.

| Aspek | Ketetapannya |
| --- | --- |
| Yang **wajib** | Lama keterlambatan terbaca; daftar **tidak boleh** menghalangi tindakan apa pun |
| Yang **bebas** | Satu halaman gabungan atau beberapa halaman terpisah; urutan kolom; cara menandai keterlambatan |

Daftar penempatan tidak sesuai berbeda sifat dari tiga lainnya: isinya bukan keterlambatan petugas,
melainkan akibat wajar dari perubahan kondisi klinis. Nadanya tidak boleh menuduh.

---

## 5. Penanganan keadaan

### 5.1 Keadaan wajib pada setiap layar daftar

| Keadaan | Yang wajib terjadi |
| --- | --- |
| Sedang memuat | Penanda memuat, bukan layar kosong yang menyesatkan |
| Kosong | Kalimat yang menjelaskan kenapa kosong dan apa yang bisa dilakukan |
| Gagal | Pesan dari server ditampilkan apa adanya bila ada, ditambah tombol coba lagi |
| Tidak berhak | Layar tidak dibuka sama sekali, bukan dibuka lalu kosong |

### 5.2 Data basi

| Layar | Risiko basi | Cara menanganinya |
| --- | --- | --- |
| `FE-INP-03` langkah Pilih Bed | **Tinggi** | Muat ulang sebelum dialog konfirmasi pemesanan |
| `FE-INP-02` papan tempat tidur | **Tinggi** | Muat ulang saat difokuskan kembali, dan wajib muat ulang sebelum dialog konfirmasi masuk |
| `FE-INP-16` daftar kerja | Sedang | Muat ulang saat difokuskan kembali. Sisa waktu pemesanan pada baris `Draft` wajib berasal dari jawaban server terakhir |
| `FE-INP-01` census | Sedang | Muat ulang saat difokuskan kembali |
| `FE-INP-07` penutupan | Sedang | Panggil `closure-readiness` **tepat sebelum** tombol tutup dijalankan |
| `FE-INP-14` pencatatan kepergian | Sedang | Muat ulang detail episode sebelum dialog konfirmasi |

**Yang tidak boleh:** menyembunyikan tombol berdasarkan data yang dimuat lima menit lalu.

### 5.3 Pengiriman ganda

Seluruh aksi yang mengubah data wajib mencegah penekanan tombol dua kali: tombol dinonaktifkan
selama permintaan berjalan.

| Aksi | Akibat bila terkirim dua kali | Perlakuan |
| --- | --- | --- |
| Menyimpan langkah Dokter (titik tulis 1) | Dua kunjungan dan dua episode untuk satu pasien | Tombol dikunci; hasil server ditampilkan apa adanya. `INV-INP-01` menolak episode aktif kedua, tetapi pesan yang terbaca petugas wajib jelas |
| Memesan tempat tidur | Permintaan kedua ditolak server | Konfirmasi sebelum kirim |
| Mengonfirmasi pasien masuk | Permintaan kedua ditolak 409 | Konfirmasi menyebut nama pasien dan tempat tidur |
| Memindahkan pasien | Sama | Konfirmasi menyebut tempat tidur asal dan tujuan |
| Membatalkan admisi | Sama | Konfirmasi menyebut bahwa pemesanan dan penempatan ikut dilepas |
| Menutup episode | Sama | Konfirmasi menyebut nama pasien dan cara pulang |
| Menutup menembus gerbang keuangan | Sama, ditambah baris laporan pengecualian | Konfirmasi **wajib** menyebut bahwa episode akan masuk laporan pengecualian |
| Mencatat kepergian pasien | Sama | Konfirmasi **wajib** menyebut bahwa tindakan **tidak dapat dibatalkan** |

### 5.4 Menangani kode 409 dan 422

| Kode | Artinya | Yang harus dilakukan layar |
| --- | --- | --- |
| 409 | Keadaan sudah berubah, misalnya tempat tidur direbut pasien lain | **Muat ulang data**, tampilkan pesan server, biarkan pengguna memilih ulang. Isian yang sudah diketik **tidak boleh hilang** |
| 422 | Aturan bisnis menolak | Tampilkan **daftar** aturan yang gagal, bukan satu kalimat umum |

Untuk 422 pada penutupan episode, layar wajib menampilkan kelima syarat beserta tanda sudah atau
belum. Untuk 422 pada pemesanan dan konfirmasi masuk, alasan penolakan Kelayakan Penempatan wajib
terbaca apa adanya, termasuk nama kamar.

### 5.5 Keadaan khusus alur berlangkah

| Keadaan | Yang wajib terjadi |
| --- | --- |
| Langkah gagal disimpan | Alur **berhenti di langkah itu**, isian utuh, pesan server apa adanya. Tidak boleh melompat ke langkah berikutnya |
| Halaman dimuat ulang di tengah alur | Langkah dan episode yang sedang dikerjakan wajib pulih dari URL, bukan hilang |
| Pemesanan gugur saat alur masih terbuka | Layar wajib menyatakannya dan mengembalikan pengguna ke langkah Pilih Bed |
| Pengguna menutup alur setelah titik tulis 1 | Wajib ada peringatan yang menyebut bahwa episode `Draft` sudah terbentuk dan dapat dilanjutkan dari daftar kerja |

---

## 6. Privasi di layar

| Aturan | Isinya |
| --- | --- |
| Isi resume pulang | Hanya pada layar detail bagi peran ber-`InpatientDischarge : Read`. **Tidak** pada daftar |
| Daftar episode dan census | Hanya nomor episode, nama pasien, lokasi, DPJP, lama dirawat, dan status. Tanpa diagnosis |
| Catatan episode | Bertanda sensitif. Tidak ditampilkan pada daftar |
| Keterangan kebutuhan isolasi | Bertanda sensitif. **Tidak** pada census maupun papan tempat tidur; hanya pada detail episode bagi peran yang berhak. Penandanya boleh tampil sebagai ikon tanpa alasannya |
| Data penjamin | Nomor kartu asuransi dan nomor peserta **tidak** ditampilkan pada daftar mana pun; hanya pada langkah Pembayaran dan pada formulir cetak |
| Formulir persetujuan tercetak | Memuat data pribadi. Halaman cetak **tidak boleh** dapat dijangkau tanpa hak akses, dan tidak boleh menyimpan salinannya di peramban |
| Contoh dan data uji | **Tidak boleh** memakai data pasien atau pegawai asli |

---

## 7. Cacat yang wajib diperbaiki lebih dulu

| Cacat | Keadaannya |
| --- | --- |
| Tombol aktifkan dan nonaktifkan tempat tidur | **Sudah diperbaiki** lewat `FE-RWI-001`. Pemanggilan `PATCH /beds/{id}/status` terbukti berjalan; nol respons 404 |

Tidak ada cacat prasyarat yang tersisa pada revision ini.

---

## 8. Pola yang dipakai ulang

Bagian ini **mengikat**. Ketiganya sudah berjalan di repository, dan mengarang pola keempat untuk
pekerjaan yang sama adalah cara paling mudah menghasilkan dua jalur yang berselisih.

| Pola | Sumber di frontend | Dipakai untuk |
| --- | --- | --- |
| **Alur pendaftaran sisi petugas** | `src/components/view/health-services/registration-management/emergency-registration/` — `patient-entry-choice-step`, `patient-selection-step`, `new-patient-form`, `payment-method-step`, `verification-step`, `registration-success-step`, `emergency-registration-stepper`, `plustek-scan-panel` | Kerangka `FE-INP-03`, termasuk pilihan penjamin |
| **Alur kiosk baru dan lama** | `src/components/view/kiosk/registration/new-patient/`, `…/old-patient/` | Urutan langkah, scan KTP, pemisahan jalur |
| **Cetak kartu pasien** | `src/components/view/kiosk/registration/patient-card/print/` | Langkah 9 jalur pasien baru |
| `InstanceAxios` beserta pembungkus `ApiResponse` | `src/lib/axiosInstance/InstanceAxios` | Seluruh pemanggilan |
| Pembungkus jawaban dan pemakluman 404 | `doctor-consultation.service.js:15-23` | Layanan yang boleh mengembalikan "belum ada" |
| Slice Redux master data | `src/lib/state/slice/health-services/master-data/` | Layar master pengaturan dan butir administrasi |
| Isian pilihan sumber daya | `health-service-select-resources.js` | Pilihan unit layanan, kelas, dokter |
| Kerangka pemanggilan Rawat Inap | `src/lib/services/health-services/inpatient-management/inpatient-api.service.js` | Seluruh pemanggilan modul ini |
| Papan tempat tidur dan penolakan penempatan | `inpatient-bed-board.jsx`, `placement-failure-list.jsx`, `use-inpatient-bed-board.jsx` | Langkah Pilih Bed dan Booking Bed |

**Yang tidak dipakai ulang:** ruang kerja antrean dokter (`useDoctorConsultationWorkspace.js`). Ruang
kerja itu berputar pada `queueId`, sedangkan pasien rawat inap tidak punya antrean.

---

## 9. Keputusan yang didelegasikan

| Decision ID | Area | Status | Batas yang diizinkan |
| --- | --- | --- | --- |
| `RWI-FE-001` | Kata untuk angka hari rawat | `DEV_DISCRETION` | Wajib menyebut jelas bahwa itu hitungan hari rawat |
| `RWI-FE-002` | Bentuk tampilan daftar pantau | `DEV_DISCRETION` | Lama keterlambatan terbaca; tidak menghalangi tindakan |
| `RWI-FE-003` | Nama dan label kesembilan langkah alur admisi | `DEV_DISCRETION` | **Urutan dan isinya mengikat** sesuai 3A.2 dan 3A.3; katanya bebas |
| `RWI-FE-004` | Bentuk penanda langkah — garis, angka, atau tab | `DEV_DISCRETION` | Langkah yang sedang berjalan dan yang sudah lewat wajib terbeda |
| `RWI-FE-005` | Tata letak Beranda | `DEV_DISCRETION` | Ketiga isi wajib pada 2B tercapai dan dapat diklik |
| Baru | Nama menu, urutan menu, dan route final | `DEV_DISCRETION` | Mengikuti konvensi `src/app/health-services/`, tunduk pada `IA-INP-01` s.d. `IA-INP-05` |
| Baru | Pemakaian tab, modal, atau drawer | `DEV_DISCRETION` | Bebas, selama aturan 5.3 dipenuhi |
| Baru | Penggabungan layar | `DEV_DISCRETION` | Sembilan belas layar bagian 2 boleh digabung, selama kemampuannya tercapai dan 2B dipenuhi |

**Yang bukan `DEV_DISCRETION`:** peta 2A, aturan keterjangkauan 2B, aturan tombol bagian 3,
kontrak alur bagian 3A, penanganan 409 dan 422 pada 5.4, dan privasi bagian 6.

---

## 10. Ketergantungan test

| Yang diuji | Jenis | Catatan |
| --- | --- | --- |
| Layar dapat dijangkau peran yang berhak, dan tidak dapat dijangkau yang tidak berhak | e2e | Menambah kasus pada `tests/e2e/route-smoke.spec.mjs` |
| Alur admisi jalur pasien baru berjalan dari langkah 1 sampai tempat tidur `Reserved` | e2e | **Wajib** — ini alur bisnis utama modul |
| Alur admisi jalur pasien lama berjalan dari pencarian sampai tempat tidur `Reserved` | e2e | **Wajib** |
| Alur yang ditinggal setelah titik tulis 1 dapat ditemukan kembali dari `FE-INP-16` dan dilanjutkan | e2e | **Wajib** — membuktikan 3A.6 |
| Kunjungan yang terbentuk membawa penjamin yang dipilih, bukan `Cash` bawaan | e2e atau pemeriksaan jaringan | **Wajib** — inilah cacat yang revision ini tutup |
| Mundur dari Dokter ke Pembayaran ditolak disertai penjelasan | e2e | Membuktikan 3A.5 |
| Konfirmasi masuk menolak tempat tidur yang keburu tidak layak | e2e | Membuktikan pemeriksaan ulang 4.3A |
| Tombol pindah nonaktif bagi dokter yang bukan DPJP aktif | e2e | Sudah ada |
| Daftar syarat penutupan tampil lengkap saat 422 | e2e | Sudah ada |

Dasar kewajiban test: `RWI-DEC-051`.

---

## 11. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 2A | `04-prd-to-mvp.md` bagian 9 `FLOW-RI-MVP-001`; `contracts/api-contract.md` `0.4.0` |
| 2B, 2C | `RWI-DEC-078`; temuan keterjangkauan revision `0.3` |
| 3 | `contracts/permission-audit-matrix.md` `0.4.0` |
| 3.2 | `RWI-DEC-076`; `RWI-OQ-045` |
| 3A | `RWI-DEC-075`, `RWI-DEC-076`, `RWI-DEC-079` |
| 3A.8 | `RWI-DEC-077`; `RWI-DEC-035`; `RWI-CAP-031` dan `DEC-INP-003` tetap terbuka |
| 4.0 | `01-existing-capability-map.md` `RWI-CAP-002`; `02-backend-architecture.md` bagian 5 |
| 4.3 | `RWI-RULE-019`, `RWI-FE-001` |
| 4.3A | `RWI-RULE-012`, `RWI-DEC-064` s.d. `RWI-DEC-066`; `contracts/validation-matrix.md` |
| 4.4 | `RWI-RULE-023`, `RWI-FE-002`, `RWI-AC-138` |
| 5.3, 5.4 | `RWI-RULE-008`, `RWI-RULE-010`, `RWI-RULE-015`, `RWI-RULE-021` |
| 6 | Kolom sensitif pada `data/data-dictionary.md` |
| `FE-INP-14` | `RWI-RULE-036`, `RWI-DEC-055` |
| 10 | `RWI-DEC-051`, `RWI-RISK-002` |

---

## 11A. Cakupan endpoint

**Bagian ini mengikat.** Setiap operasi pada `contracts/api-contract.md` wajib dimiliki tepat satu
layar, atau dinyatakan **sengaja tidak dipakai** beserta alasannya. Endpoint tak bertuan adalah
cacat blueprint — revision `0.3` meninggalkan sembilan di antaranya.

### Yang menganggur pada revision `0.3`, dan pemiliknya sekarang

| Operasi | Pemilik baru |
| --- | --- |
| `GET /episodes` | `FE-INP-16` |
| `GET /episodes/filters/metadata` | `FE-INP-16` |
| `GET /episodes/summary` | `FE-INP-19` |
| `GET /census/summary` | `FE-INP-19` |
| `GET /census/filters/metadata` | `FE-INP-01` |
| `PUT /episodes/{id}` | `FE-INP-03` titik tulis 3 |
| `PATCH /episodes/{id}/cancel` | `FE-INP-17` |
| `POST /bed-occupancies/reservations` | `FE-INP-03` langkah Booking Bed |
| `PATCH /bed-occupancies/reservations/{id}/cancel` | `FE-INP-03` langkah Booking Bed, dan `FE-INP-02` |

### Endpoint milik modul lain yang dikonsumsi modul ini

| Operasi | Pemilik data | Dipakai di |
| --- | --- | --- |
| `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts` | PatientManagement | `FE-INP-03` langkah Pendaftaran |
| `POST /patient-insurances`, `POST /patient-company-guarantors` | PatientManagement | `FE-INP-03` langkah Pembayaran |
| `POST /patient-encounters` | RegistrationManagement | `FE-INP-03` titik tulis 1 |

**Catatan penting tentang `POST /patient-encounters`.** Inilah satu-satunya alasan cacat penjamin
tertutup tanpa perubahan backend: endpoint itu membuat baris kunjungan **dan** baris penjamin
sekaligus, lalu menyetel ringkasan pembayaran kunjungan. Karena `EncounterId` menjadi terisi,
`POST /episodes` tidak pernah lagi menempuh jalur yang membuat kunjungan sendiri.

**Butir terbuka `RWI-OQ-046`:** jalur "buka admisi tanpa `EncounterId`" masih ada di backend dan
masih menanam cara bayar tunai beserta kunjungan tanpa penjamin. Setelah revision ini tidak ada
layar yang menempuhnya, tetapi jalurnya tetap terbuka bagi pemanggil lain. Perlu diputuskan apakah
jalur itu ditutup. Owner: Backend/API bersama Product/Domain. **Di luar** wewenang dokumen ini.

---

## 12. Amandemen terbatas revision `0.8` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| Frontend SHA | `1ce219b40f8e411f3c4e66975626ab33ae81616a` |
| Masukan | `02-backend-architecture.md` `0.8` bagian 11; kontrak `0.9.0` |
| Peta menu | [`../02-module-map.md`](../02-module-map.md) bagian 3 revision `2` — **nol butir menu baru** dari sub-modul ini |

### 12.1 Kebutuhan layar

| ID | Layar | Tujuan | Pemakai utama | Keadaan |
| --- | --- | --- | --- | --- |
| `FE-INP-21` | **Dokter Pendukung** — dialog pada panel penugasan `FE-INP-04` | Melibatkan konsulen, memanggil dokter jaga, membuat penugasan singkat, mengakhiri penugasan | Kepala ruangan, supervisor | **Baru** |
| `FE-INP-22` | **Resume delapan bagian** — perubahan formulir `FE-INP-06` | Tiga isian baru dan tombol "Isi dari data klinis" | DPJP | **Rework** |
| `FE-INP-23` | **Peringatan dan akibat penutupan** — perubahan `FE-INP-07` | Menampilkan apa yang akan terkunci atau batal, lalu hasilnya | Petugas admisi, supervisor | **Rework** |
| `FE-INP-24` | **Daftar pantau pesanan tertunda tertagih** — daftar baru pada `FE-INP-09` | Menindaklanjuti pesanan yang tidak dibatalkan saat penutupan | Supervisor, admisi, billing | **Baru** |

Census `assignedToMe` **tidak** mengubah layar sub-modul ini; ia dipakai `FE-DOK-09` milik `dokter-rawat-inap`.

### 12.2 Jalan masuk

| Layar | Induk | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-INP-21` | Tombol "Tambah Dokter Pendukung" pada panel Penugasan Dokter `FE-INP-04`; tombol "Akhiri" pada baris konsulen/dokter jaga | `InpatientEpisode : Update`; tombol **disembunyikan** bagi selain kepala ruangan/supervisor |
| `FE-INP-22` | Bagian Resume pada `FE-INP-06`, dan tab Resume Medis `FE-DOK-12` yang memakai kontrak sama | `InpatientDischarge : Read`/`Update`/`Sign` |
| `FE-INP-23` | `FE-INP-07` | `InpatientDischarge : Read`/`Close`/`CloseOverride` |
| `FE-INP-24` | Kartu daftar pantau pada `FE-INP-09` | `InpatientMonitoring : Read` |

### 12.3 Skema fitur per layar

#### 12.3.1 `FE-INP-21` Dokter Pendukung

```text
Tambah Dokter Pendukung — Budi S. · Melati 302/2
─────────────────────────────────────────────────
Dokter            [ dr. Ahmad, Sp.PD        ▾ ]
Peran             (•) Konsulen   ( ) Dokter jaga
Tujuan            (•) Biasa      ( ) Penulisan catatan terlambat
Mulai             [ 16/09/2026 10.00 ]
Selesai           [                  ]  (wajib untuk catatan terlambat)
Alasan            [ Konsultasi hiperglikemia            ]
                                  [Batal] [Simpan]
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong | Keadaan gagal |
| --- | --- | --- | --- | --- | --- |
| Pilihan dokter | Dokter aktif | Endpoint opsi dokter yang sudah dipakai admisi | — | "Tidak ada dokter aktif" | Pesan dan tombol simpan nonaktif |
| Tujuan | Memilih "Penulisan catatan terlambat" **mengunci** peran ke Dokter jaga, mewajibkan Selesai, dan mengisi Mulai dengan waktu sekarang | — | — | — | — |
| Simpan | Kirim + `Idempotency-Key` | `POST episodes/{id}/doctor-assignments/supporting` | `InpatientEpisode : Update` | — | `400`/`409`/`422` ditampilkan per isian |
| Panel penugasan | Baris konsulen dan dokter jaga, periode, alasan, **label "Penugasan singkat"** bila tujuan `LateDocumentation`; tombol Akhiri | `GET episodes/{id}/doctor-assignments`, `PATCH …/end` | `InpatientEpisode : Read`/`Update` | "Belum ada dokter pendukung" | "Riwayat penugasan tidak dapat dimuat" |

#### 12.3.2 `FE-INP-22` Resume delapan bagian

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong / gagal |
| --- | --- | --- | --- | --- |
| Delapan bagian urutan PRD bagian 21 | Diagnosis; Ringkasan Perawatan; **Pemeriksaan Penting**; Tindakan; Obat/Terapi; **Kondisi Saat Pulang**; Rencana Kontrol; **Edukasi** | `GET/PUT discharges/{episodeId}/summary` | `InpatientDischarge : Read`/`Update` | Resume belum ada → form kosong |
| "Isi dari data klinis" | Mengisi **hanya isian yang kosong** di layar; isian yang sudah diketik dokter tidak ditimpa tanpa konfirmasi; label sumber di bawah setiap isian terisi | `GET /summary-prefill` | `InpatientDischarge : Read` | Sumber `Unavailable` → "Data {sumber} tidak dapat dimuat" pada isian itu saja |
| Tanda tangan | Kalimat tetap "Menandatangani resume tidak menutup episode" | `PATCH /summary/sign` | `InpatientDischarge : Sign` | `403` → pesan DPJP aktif |

#### 12.3.3 `FE-INP-23` Peringatan dan akibat penutupan

```text
Syarat penutupan                    ✓ 5 dari 5 terpenuhi
Perlu diketahui — tidak menahan penutupan
  ! 1 konsep catatan dokter akan terkunci "Tidak Ditandatangani" (dr. Yoga — SOAP)
  ! 1 pesanan tindakan belum dilaksanakan akan dibatalkan
  ! 1 dosis obat lewat jadwal belum dicatat
                                              [Tutup Episode]
```

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan gagal |
| --- | --- | --- | --- | --- |
| Syarat | Tidak berubah | `GET closure-readiness` | `InpatientDischarge : Read` | Tidak berubah |
| Peringatan | `Warnings[]` dengan rincian dapat dibuka | Sama | Sama | Peringatan gagal dimuat → "Peringatan tidak dapat dimuat"; tombol Tutup **tetap** mengikuti syarat, bukan peringatan |
| Hasil | Setelah berhasil: ringkasan `SideEffects` | Response `close` | `InpatientDischarge : Close` | `500` → "Penutupan gagal disimpan, coba lagi" — tidak ada yang berubah |

Peringatan memakai warna netral atau kuning (`DEV_DISCRETION`), **tidak** merah yang terbaca sebagai penghalang.

#### 12.3.4 `FE-INP-24` Daftar pantau pesanan tertunda tertagih

| Wilayah | Isinya | Sumber data | Hak akses | Keadaan kosong / gagal |
| --- | --- | --- | --- | --- |
| Tabel | Pasien, episode, unit, tindakan, penginput, waktu pesan, waktu tutup, nomor tagihan | `GET monitoring/billed-pending-procedure-orders` | `InpatientMonitoring : Read` | "Tidak ada pesanan tertunda yang sudah ditagih" / "Daftar tidak dapat dimuat" |
| Tindak lanjut | Hanya baca; kalimat "Tindak lanjut bersama Billing" | — | — | — |

### 12.4 Traceability

| Layar | Keputusan | Acceptance |
| --- | --- | --- |
| `FE-INP-21` | `RWI-DEC-099`, `130` | Acceptance 18.2 |
| `FE-INP-22` | `RWI-DEC-112` | Acceptance 18.3 |
| `FE-INP-23` | `RWI-DEC-138`, `143`; `INT-KEP-15` | Acceptance 18.4 |
| `FE-INP-24` | `RWI-DEC-143` (c) | Acceptance 18.4 |

---

## 13. Amandemen revision `0.9` / kontrak `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
| --- | --- |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Frontend SHA | `22ad67330` |
| Masukan | `02-backend-architecture.md` `0.9` bagian 12; kontrak `0.10.0`; PRD Finishing v`0.4` `EPIC-RWF-05`, `08`, `09` |
| Peta menu | [`../02-module-map.md`](../02-module-map.md) bagian 7.3 revision `4` — **dua butir menu baru**: Butir Persiapan Bedah di Master Data, dan Laporan Rawat Inap sebagai butir tingkat dua Rawat Inap ke-10 (`IA-INP-05` menjadi sepuluh, `RWI-DEC-214`) |

### 13.1 Kebutuhan layar

| ID | Layar | Jenis | Pemakai utama | Kemampuan | Keadaan |
|---|---|---|---|---|---|
| `FE-INP-25` | **Pemesanan Ruangan Bedah** — tab Bedah Operasi dan Bedah Obgyn | Isi menu ketujuh ruang kerja keperawatan `FE-KEP-07`, menggantikan *placeholder* bagian bedah `FE-KEP-17` | Perawat, dokter bangsal | `CAP-RWF-07` | **Baru** |
| `FE-INP-26` | **Daftar Pesanan Ruang Bedah pasien** — status, alasan, aksi per kasus | Di bawah formulir `FE-INP-25`; kartu ringkas pada Detail Episode `FE-INP-04` | Perawat, dokter, kepala ruangan | `CAP-RWF-08`, `CAP-RWF-22` | **Baru** |
| `FE-INP-27` | **Catatan Pra-Operasi** — mode pengirim (bangsal) dan mode penerima (OK) | Bangsal: sub-menu Catatan Pra-Operasi di `FE-KEP-24` dan aksi baris `FE-INP-26`. OK: bagian pada tab Persiapan detail kasus OK | Perawat bangsal, perawat OK | `CAP-RWF-08` | **Baru** |
| `FE-INP-28` | **Pasca Operasi** — ringkasan operasi baca-saja dan serah terima masuk (terima/tolak) | Laci dari aksi baris `FE-INP-26`; di ruang kerja dokter lewat penanda "Pasca operasi" pada panel Konteks pasien (`RWI-DEC-213`) | Perawat unit tujuan, dokter | `CAP-RWF-08`, `CAP-RWF-19` | **Baru** |
| `FE-INP-29` | **Permintaan Admisi dari Kamar Pulih** | Daftar pada layar Admisi Rawat Inap `FE-INP-03`; tombol "Admisi" membuka alur berlangkah yang terisi awal | Petugas admisi | `CAP-RWF-18` | **Baru** |
| `FE-INP-30` | **Dua daftar pantau** — serah terima pasca operasi tertunda; permintaan admisi tertunda | Daftar Pantau `FE-INP-09`, kelompok `episode-rawat-inap` | Kepala ruangan, admisi, OK | `CAP-RWF-08`, `CAP-RWF-18` | **Baru** |
| `FE-INP-31` | **Serah Terima Transfer** (`P2`) | Banner "Serah terima tertunda" pada `FE-INP-04` dan `FE-KEP-07`; laci dokumen | Perawat unit asal dan tujuan | `CAP-RWF-16` | **Baru** — mengganti "Integrasi belum tersedia" |
| `FE-INP-32` | **Laporan Rawat Inap** — wadah laporan; isi pertama Laporan Transfer Ruangan (`P2`) | Butir menu tingkat dua ke-10 (`RWI-DEC-214`, `RWI-DEC-215`) | Kepala ruangan, manajemen | `CAP-RWF-23` | **Baru** |
| `FE-INP-33` | **Tolak Order Operasi** — dialog, label "Ditolak" merah, kolom "Ditolak" di laporan OK | Detail kasus pada menu Operasi → Kasus Operasi; Operasi → Laporan Operasi | Petugas penjadwalan OK | `CAP-RWF-22` | **Rework** layar OK |
| `FE-INP-34` | **Master Butir Persiapan Bedah** | Butir menu Master Data | Admin Master Data | `CAP-RWF-08` | **Baru** |

Perubahan kecil tanpa ID baru: Pengaturan Rawat Inap `FE-INP-12` bertambah dua isian ambang (menit); alur admisi berlangkah `FE-INP-03` menerima pengisian awal dari `FE-INP-29`.

### 13.2 Peta butir menu dan jalan masuk

Butir menu milik sub-modul ini (peta seluruh modul di `02-module-map.md` 7.3):

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Butir Persiapan Bedah | 2 | Pelayanan Kesehatan → Master Data | `/health-services/master-data/surgical-preparation-items` | `FE-INP-34` | `SurgicalPreparationItem : Read` | Baru |
| Laporan Rawat Inap | 2 | Pelayanan Kesehatan → **Rawat Inap** | `/health-services/inpatient-management/reports` (usulan; `DEV_DISCRETION`) | `FE-INP-32` | Salah satu permission laporan rawat inap — hari ini `InpatientReport : ReadRoomTransfer` | Baru — butir ke-10, `IA-INP-05` diamendemen `RWI-DEC-214` |

| Layar anak | Jalan masuk | Butir hak akses penjaga |
|---|---|---|
| `FE-INP-25` | `FE-KEP-07` menu 7 "Pemesanan Ruangan Bedah" (tab tetap dua seperti V1, `RWI-DEC-172`) | Tombol Pesan: `OperatingRoomCase : Create` |
| `FE-INP-26` | Sama; kartu "Operasi" pada `FE-INP-04` | `OperatingRoomCase : Read` |
| `FE-INP-27` | Bangsal: `FE-KEP-24` sub-menu Catatan Pra-Operasi; aksi baris "Pra-operasi" pada `FE-INP-26`. OK: tab Persiapan detail kasus | `OperatingRoomWardPreOp : Read`; tombol Kirim `: Send`; tombol Konfirmasi `: Confirm` |
| `FE-INP-28` | Aksi baris "Pasca operasi" pada `FE-INP-26`; baris pada daftar `FE-INP-30` | `OperatingRoomCase : Read`; tombol Terima/Tolak `OperatingRoomHandover : Receive` |
| `FE-INP-29` | `FE-INP-03`; kartu `FE-INP-30` | `InpatientAdmissionReferral : Read`; tombol Admisi `InpatientEpisode : Create` |
| `FE-INP-30` | `FE-INP-09` | `InpatientMonitoring : Read` |
| `FE-INP-31` | Banner pada `FE-INP-04`, `FE-KEP-07`; daftar per unit pada `FE-INP-09` | `TransferHandover : Read`; Kirim `: Send`; Terima `: Receive` |
| `FE-INP-32` | Butir menu "Laporan Rawat Inap" (`RWI-DEC-214`, `215`) | Butir tampil bila memegang salah satu permission laporan rawat inap (hari ini `InpatientReport : ReadRoomTransfer`); tombol Ekspor `: ExportRoomTransfer` |
| `FE-INP-33` | Operasi → Kasus Operasi → detail | Tombol Tolak `OperatingRoomCase : Reject`, tampil hanya pada kasus Diminta |

**Keterjangkauan.** Perawat: Census → pasien → `FE-KEP-07` → menu 7 = 3 klik ke `FE-INP-25`/`26`. Petugas admisi: Admisi Rawat Inap → daftar "Dari Kamar Pulih" = 2 klik. Perawat ICU yang menunggu pasien dari OK: Daftar Pantau → "Serah terima pasca operasi tertunda" = 2 klik.

### 13.3 Keputusan rupa yang belum dibuat

Tidak menahan backend; menahan task frontend layar yang disebut.

| ID | Pertanyaan | Opsi | Rekomendasi agent | Pemilik |
|---|---|---|---|---|
| `UI-RWF-03` | Di mana dokter membaca ringkasan operasi (`FE-INP-28`) di ruang kerja dokter `FE-DOK-09`? | ~~A. Kartu pada tab Ringkasan Pasien~~ — **tab itu tidak ada** (koreksi `RWI-DEC-213`) | — | **Diputuskan `RWI-DEC-213`: penanda "Pasca operasi" pada panel Konteks pasien** |
| `UI-RWF-04` | Di mana Laporan Transfer Ruangan (`FE-INP-32`) dibuka? | A. Tautan di Dashboard; B. butir menu ke-10; C. tab Daftar Pantau | A (tidak dipilih) | **Diputuskan `RWI-DEC-214`, `215`: butir menu ke-10 "Laporan Rawat Inap" sebagai wadah laporan** |

### 13.4 Skema fitur per layar

#### 13.4.1 `FE-INP-25` Pemesanan Ruangan Bedah

```text
Pemesanan Ruangan Bedah — Budi S. · RM 00-12-34-56 · Melati 302/2
[ Bedah Operasi ] [ Bedah Obgyn (SC/Caesar) ]
─────────────────────────────────────────────────────────────
Tindakan operasi*   [ Appendektomi — dr. Hadi, Sp.B (order 30/09) ▾ ]
Dokter operator     dr. Hadi, Sp.B               (dari order, tidak dapat diubah)
Perkiraan tindakan  Ditanggung · Rp … — perkiraan, tagihan final di kasir
                    (jasa anestesi, sewa kamar operasi, dan bahan ditagihkan setelah operasi)
Tanggal & jam*      [ 03/10/2026 08.00 ]
Jenis anestesi*     [ Umum ▾ ]        Prioritas* [ Rutin ▾ ]   Jenis* (•) Elektif ( ) Darurat
Sisi tubuh          ( ) Kiri ( ) Kanan ( ) Bilateral (•) Tidak berlaku
Perkiraan durasi*   [ 60 ] menit
Indikasi*           [ ............................................. ]
Keterangan          [ ............................................. ]
Penginput           Ns. Rina (akun login)
                                                     [ Pesan Ruang Bedah ]
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Pilihan tindakan | Order tindakan operasi aktif milik kunjungan episode | `GET patient-procedures?encounterId=&procedureStatus=` | `PatientProcedure : Read` | Kosong → "Tindakan operasi belum dipesan dokter. Minta dokter memesan tindakan lebih dulu." dan tombol Pesan nonaktif |
| Perkiraan tindakan | `UnitPrice` dan `CoverageStatus` order yang dipilih, berlabel perkiraan; keterangan komponen OK yang menyusul (`RWI-DEC-218`, `219`) | Sama, `GET patient-procedures` (`PatientProcedureResponse`) | `PatientProcedure : Read` | Harga tidak ada → "tarif belum tersedia"; tombol Pesan tetap aktif (`RWI-DEC-220` butir 5) |
| Tab Obgyn | Sama; jenis layanan "Obstetri" tampil sebagai label tetap | — | — | — |
| Tombol Pesan | Kirim `SurgeryBookingRequest`; terkunci selama permintaan berjalan (cegah kirim ganda, `Idempotency-Key`) | `POST episodes/{id}/surgery-bookings` | `OperatingRoomCase : Create` | `422 INP-SRG-001` → pesan server; `INP-SRG-002` → "Pemesanan hanya untuk pasien yang sedang dirawat" |

#### 13.4.2 `FE-INP-26` Daftar Pesanan Ruang Bedah pasien

```text
Pesanan Ruang Bedah pasien ini                                [ Muat ulang ]
┌──────────────┬───────────────┬────────────────┬────────────────────────┬─────────────────────────┐
│ No. kasus    │ Tindakan      │ Status         │ Keterangan             │ Aksi                    │
├──────────────┼───────────────┼────────────────┼────────────────────────┼─────────────────────────┤
│ OK-2026-0142 │ Appendektomi  │ Terjadwal      │ 03/10 08.00, OK 2      │ [Pra-operasi]           │
│ OK-2026-0139 │ Debridemen    │ ■ Ditolak      │ "Hasil lab belum ada"  │ [Pesan ulang]           │
│              │               │                │ oleh Ns. Dewi, 30/09   │                         │
│ OK-2026-0120 │ ORIF femur    │ Selesai        │ Serah terima diterima  │ [Pasca operasi]         │
└──────────────┴───────────────┴────────────────┴────────────────────────┴─────────────────────────┘
Label "Diminta" tidak menjamin ruang.
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Tabel | Delapan label status (`FR-RWF-044`); alasan untuk Ditunda/Dibatalkan/Ditolak; penolak dan waktu untuk Ditolak; status pra-operasi dan serah terima | `GET operating-room-management/cases?encounterId=` | `OperatingRoomCase : Read` | Kosong → "Belum ada pesanan ruang bedah." Gagal → "Daftar operasi tidak dapat dimuat. Coba lagi." |
| Pesan ulang | Membuka `FE-INP-25` dengan order yang sama terpilih | — | `OperatingRoomCase : Create` | — |
| Data basi | Tidak ada dorongan waktu nyata; tombol Muat ulang dan penyegaran saat menu dibuka (`AC-RWF-041`) | — | — | — |

#### 13.4.3 `FE-INP-27` Catatan Pra-Operasi

```text
Catatan Pra-Operasi — OK-2026-0142 · Appendektomi · Sisi pesanan: Tidak berlaku   Versi 2 (perlu diperbarui → baru)
Tanda vital (potret saat dikirim)   TD 130/85 · N 88 · RR 20 · S 36,8 · SpO₂ 98%   dicatat 02/10 21.00
Nyeri                               3/10 (NRS)                                      dicatat 02/10 21.00
┌───────────────────────────────┬───────────┬──────────┬─────────────────────┐
│ Butir                          │ Pengirim  │ Penerima │ Catatan             │
├─ Verifikasi pasien ───────────┼───────────┼──────────┼─────────────────────┤
│ Gelang identitas terpasang *   │ [✓]       │ [ ]      │                     │
├─ Persiapan fisik ─────────────┼───────────┼──────────┼─────────────────────┤
│ Puasa *                        │ [✓]       │ [ ]      │ sejak 22.00         │
└───────────────────────────────┴───────────┴──────────┴─────────────────────┘
Penandaan area operasi  [gambar tubuh depan/belakang — klik untuk titik]  Sisi: (•) Tidak berlaku
Riwayat versi: v1 (perlu diperbarui, 01/10)            [ Simpan draf ] [ Kirim ]   (OK: [ Konfirmasi ])
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Potret | Tanda vital dan nyeri terakhir; sesudah dikirim dibekukan | `GET …/ward-pre-op` | `OperatingRoomWardPreOp : Read` | Belum ada tanda vital → "Catat tanda vital pasien lebih dulu" dan Kirim nonaktif |
| Checklist | Kelompok dan butir dari master; kolom Pengirim aktif di bangsal, kolom Penerima aktif di OK | Sama; `PUT /draft`, `PATCH /confirm` | `: Send`, `: Confirm` | Master kosong → "Butir persiapan belum diatur di Master Data" |
| Penandaan | Titik pada gambar tubuh, sisi, keterangan lokasi; tanpa unggah foto | Sama | `: Send` | Sisi berbeda → pesan `OPR-WPO-001` di bawah pilihan sisi |
| Versi | Banner kuning "Perlu diperbarui setelah penundaan" bila versi terbaru `NeedsUpdate`; tombol "Buat versi baru" menyalin butir lama | `GET …/versions` | `: Read` | — |
| Akun sama | Bila akun login = pengirim, tombol Konfirmasi tidak tampil dan tertulis "Konfirmasi harus oleh akun lain" | — | — | — |

#### 13.4.4 `FE-INP-28` Pasca Operasi

```text
Pasca Operasi — OK-2026-0120 · ORIF femur kanan · dr. Hadi, Sp.B        (baca saja)
Diagnosis pasca bedah   Fraktur femur kanan 1/3 tengah
Temuan / komplikasi     … / Tidak ada
Perdarahan              350 ml        Drain & implan  Drain 1 di paha kanan; plate 8 hole
Rencana pasca bedah     Elevasi tungkai, cek Hb besok pagi
Anestesi                Umum (rencana: Umum)   Kamar pulih  Aldrete 9 → Rawat Inap
Instruksi serah terima  …
── Serah terima ─────────────────────────────────────────────
Dikirim Ns. Andi (OK) 01/10 13.10 ke Melati      Status: Menunggu diterima
[ Terima ]  [ Tolak… ]
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Ringkasan | Isian `PostOperativeSummaryResponse`; tanpa tombol ubah | `GET cases/{id}/post-operative-summary` | `OperatingRoomCase : Read` | `ReportFinal = false` → "Laporan operasi belum final". Gagal → "Ringkasan operasi tidak dapat dimuat" |
| Serah terima | Status, pengirim, waktu; Terima dan Tolak (alasan wajib) | `PATCH …/handovers/{id}/accept` | `OperatingRoomHandover : Receive` | `OPR-HO-001` → tombol Terima terkunci dengan pesan dan tautan ke Perpindahan Pasien `FE-INP-05`; `OPR-HO-002` → "Pengirim tidak dapat menerima serah terima sendiri" |

#### 13.4.5 `FE-INP-29` Permintaan Admisi dari Kamar Pulih

```text
Admisi Rawat Inap   [ Admisi baru ]   [ Dari Kamar Pulih (2) ]
┌────────────┬──────────────┬───────────────┬──────────────┬──────────┬───────────┬─────────┐
│ Pasien     │ Kunjungan    │ Operasi       │ Dokter       │ Tujuan   │ Menunggu  │ Aksi    │
│ Sari W.    │ Poli Bedah   │ Kolesistektomi│ dr. Hadi     │ Rawat    │ ■ 45 mnt  │[Admisi] │
└────────────┴──────────────┴───────────────┴──────────────┴──────────┴───────────┴─────────┘
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Daftar | Permintaan `Pending`; lamanya menunggu merah bila melewati ambang | `GET admission-referrals` | `InpatientAdmissionReferral : Read` | Kosong → "Tidak ada pasien dari kamar pulih yang menunggu admisi." |
| Admisi | Membuka alur berlangkah `FE-INP-03` dengan pasien, kunjungan asal, dan usulan DPJP terisi; penjamin, kelas, DPJP, deposit, bed tetap diisi petugas | `GET admission-referrals/{id}`; `POST episodes` + `AdmissionReferralId` | `InpatientEpisode : Create` | `INP-ADM-REF-002` → "Permintaan ini sudah selesai atau dibatalkan" dan daftar dimuat ulang |
| Admisi biasa | Bila pasien yang dipilih di "Admisi baru" punya permintaan, server menolak `409 INP-ADM-REF-001`; layar menawarkan "Buka dari permintaan" | — | — | — |

#### 13.4.6 `FE-INP-30` Dua daftar pantau

| Daftar | Kolom | Sumber data | Kosong |
|---|---|---|---|
| Serah terima pasca operasi tertunda | Pasien, kasus, unit tujuan, unit pasien sekarang, dikirim, lamanya menunggu, tanda "pasien belum di unit tujuan" | `GET monitoring/pending-surgical-handovers` | "Tidak ada serah terima pasca operasi yang tertunda." |
| Permintaan admisi tertunda | Pasien, kasus, dokter, lamanya menunggu | `GET monitoring/pending-admission-referrals` | "Tidak ada permintaan admisi yang tertunda." |

Hak akses `InpatientMonitoring : Read`. Letak di dalam kelompok `episode-rawat-inap` ditetapkan `RWI-DEC-216`: sesudah `FE-INP-24`, berurutan `FE-INT-03`, serah terima pasca operasi tertunda, permintaan admisi tertunda (`02-module-map.md` 7.3).

#### 13.4.7 `FE-INP-31` Serah Terima Transfer (`P2`)

```text
⚠ Serah terima tertunda — dari Melati ke ICU, dipindah 01/10 14.00           [ Buka ]
Laci: Kondisi pasien (SOAP) · GCS · Tanda vital · Nyeri & risiko jatuh · Balance cairan ·
      Barang diserahkan · Instruksi khusus · Pelaksana · Penerima         [ Kirim ] / [ Terima ] [ Tolak… ]
```

Nilai klinis tampil dari pencatatan terakhir dan dibekukan saat dikirim. Banner tidak pernah mengunci tombol lain. Sumber `clinical-management/transfer-handovers`; kosong → banner tidak tampil.

#### 13.4.8 `FE-INP-32` Laporan Transfer Ruangan (`P2`)

```text
Periode* [01/10/2026]–[07/10/2026]  Unit asal [▾] Unit tujuan [▾] Kelas [▾]  [Tampilkan] [Ekspor Excel]
Waktu        No. RM     Pasien   Asal (kelas/bed)   Tujuan (kelas/bed)   Alasan         Pencatat   Jenis
01/10 14.00  00-12-34   Budi S.  II / Melati 302-2  ICU / ICU-3          Perburukan     Ns. Rina   Transfer
02/10 09.10  00-55-01   Sari W.  I / Mawar 101-1    I / Mawar 101-2      Salah catat    Ka. Ruang  Koreksi
```

Sumber `GET reports/room-transfers`; Ekspor `GET …/export` hanya tampil bagi pemegang `ExportRoomTransfer`. Kosong → "Tidak ada transfer pada periode ini." Periode > 31 hari → pesan `VAL-RWF-90`.

#### 13.4.9 `FE-INP-33` Tolak Order Operasi (layar OK)

Tombol "Tolak" pada detail kasus berstatus Diminta membuka dialog alasan (10–500 karakter). Kasus Ditolak tampil dengan label merah, alasan, penolak, dan waktu; semua tombol ubah disembunyikan. Laporan Operasi menampilkan kolom "Ditolak" terpisah dari "Dibatalkan". Sumber `PATCH cases/{id}/reject`, `GET reports/operations`.

#### 13.4.10 `FE-INP-34` Master Butir Persiapan Bedah

Daftar dengan saringan kelompok dan aktif; formulir kode, kelompok, nama butir, wajib, urutan, keterangan; tombol aktif/nonaktif. Sumber `master-data/surgical-preparation-items`. Kosong → "Belum ada butir persiapan bedah."

### 13.5 Ketergantungan test

| Layar | Test yang menahan | Catatan |
|---|---|---|
| `FE-INP-25`, `26` | `UAT-RWF-05`, `24`; `AC-RWF-040`, `041`, `048`, `085`, `099` | Butuh kasus OK sungguhan |
| `FE-INP-27` | `AC-RWF-042`, `045`, `046`, `093`, `094`; `UAT-RWF-21` | Dua akun berbeda |
| `FE-INP-28` | `AC-RWF-043`, `047`, `081`, `082`; `UAT-RWF-13`, `17` | Pasien dipindah ke ICU lewat `FE-INP-05` |
| `FE-INP-29`, `30` | `AC-RWF-080`, `087`, `088`, `089`; `UAT-RWF-16` | Persetujuan OK sudah ada (`RWI-DEC-208`) |
| `FE-INP-31` | `AC-RWF-071`, `072`; `UAT-RWF-14` | `P2` |
| `FE-INP-32` | `AC-RWF-086`, `100`, `RWI-AC-340`; `UAT-RWF-20` | `P2` |

### 13.6 Traceability bagian 13

| Layar | Requirement | Keputusan |
|---|---|---|
| `FE-INP-25`, `26` | `FR-RWF-040` s.d. `044`, `086` | `RWI-DEC-175`, `176`, `204` |
| `FE-INP-27` | `FR-RWF-045`, `048`, `090` | `RWI-DEC-173`, `174`, `199` |
| `FE-INP-28` | `FR-RWF-046`, `049`, `081`, `082` | `RWI-DEC-177`, `189`, `197` |
| `FE-INP-29`, `30` | `FR-RWF-080`, `088`, `089` | `RWI-DEC-201` |
| `FE-INP-31` | `FR-RWF-071` | `RWI-DEC-182`, `189` |
| `FE-INP-32` | `FR-RWF-087` | `RWI-DEC-205` |
| `FE-INP-33` | `FR-RWF-086` | `RWI-DEC-204` |
| `FE-INP-34` | `FR-RWF-045` | `RWI-DEC-173` butir 3 |

### 13.7 Penyelarasan decision log revision `31` ★ 2 Oktober 2026

| Keputusan | Layar | Akibat |
|---|---|---|
| `RWI-DEC-213` (`UI-RWF-03`) | `FE-INP-28` | Jalan masuk dokter: penanda "Pasca operasi" pada panel Konteks pasien `FE-DOK-09` (dirancang `dokter-rawat-inap` 11.5). Laci terbuka mode baca-saja, tanpa Terima/Tolak. Rujukan lama "tab Ringkasan Pasien" pada 13.3 keliru |
| `RWI-DEC-214`, `RWI-DEC-215` (`UI-RWF-04`) | `FE-INP-32` | Menjadi halaman wadah **Laporan Rawat Inap** di butir menu tingkat dua ke-10. Isi pertamanya Laporan Transfer Ruangan (skema 13.4.8). Laporan rawat inap berikutnya masuk halaman ini tanpa butir menu baru. Butir tidak tampil bagi pengguna tanpa permission laporan rawat inap |
| `RWI-DEC-216` (`UI-RWF-05`) | `FE-INP-30` | Kedua daftar di akhir kelompok episode Daftar Pantau, sesudah `FE-INT-03` |
| `RWI-DEC-218`, `RWI-DEC-219` | `FE-INP-25` | Perkiraan tarif tindakan dari order yang dirujuk, beserta keterangan komponen OK yang ditagihkan setelah operasi |
| `RWI-DEC-207` | — | Tidak ada layar baru di sub-modul ini; baris operasi kunjungan asal tampil di Tagihan Pasien `FE-KEP-23` |
| `RWI-DEC-208`, `RWI-DEC-217` | `FE-INP-29`, `FE-INP-33` | Tidak lagi tertahan persetujuan OK; prioritas `P1` |

**`IA-INP-05` versi sepuluh butir.** Tabel aturan `IA-INP` di atas menulis "paling banyak sembilan"; sejak `RWI-DEC-214` angkanya **sepuluh**. Sisa kuota nol.

Rupa di dalam letak yang sudah diputuskan `DEV_DISCRETION` (`RWI-FE-006`).

---

## 14. Amandemen revision `0.10` / kontrak `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
| --- | --- |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`). Bagian 13 (Finishing) tetap `approved` (`RWI-DEC-221`) |
| Frontend SHA | `27889662a` (branch `HamzahV2`) saat desain; **diperiksa ulang pada HEAD `dd2cbf7c`** (7 Oktober 2026). Commit itu hanya memasukkan perubahan pemesanan bedah, penunjang, modal keperawatan, dan satu prop di `inpatient-episode-detail-view.jsx:1130`; baris yang dikutip (`:610`, `:613`, `:624`, `:672`, `:714`) tidak bergeser. Working tree sesudahnya menyentuh laci pra-operasi/pasca operasi dan modal keperawatan, bukan berkas yang dikutip |
| Masukan | `02-backend-architecture.md` `0.10` bagian 13; kontrak `0.11.0`; `PRD-RWI-ADMISI-001` v`0.2` bagian 12.1 dan Lampiran A; brief pemilik 7 Oktober 2026 (tombol sesudah Workspace Dokter, template Workspace Keperawatan, isian form mengikuti V1) |
| Peta menu | [`../02-module-map.md`](../02-module-map.md) bagian 8.3 — **nol butir menu baru**; `IA-INP-05` tetap sepuluh. Seluruh layar adalah layar anak Detail Episode `FE-INP-04` |
| Label | Tombol "Workspace PPRI" (`RWI-DEC-245`). Judul "Ruang Kerja PPRI" dan subjudul "Penerimaan Pasien Rawat Inap" dipakai sebagai `draft` sampai `RWI-OQ-121` dijawab |

### 14.1 Kebutuhan layar

| ID | Layar | Jenis | Pemakai utama | Kemampuan | Gelombang | Keadaan |
|---|---|---|---|---|---|---|
| `FE-INP-35` | **Workspace PPRI** — kerangka: header pasien, navigasi kiri "DOKUMEN ADMISI" sembilan menu berlencana, ringkasan kelengkapan, batas keadaan | Layar anak `FE-INP-04` | Petugas admisi, CRO, perawat, kepala ruangan, kasir, supervisor | `CAP-RWA-01` | `RWA-MVP-1` | **Baru** |
| `FE-INP-36` | **General Consent** — tab Surat Persetujuan 12 butir dan tab Formulir General Consent V1; **cetak saja** | Bagian `FE-INP-35` | Petugas admisi | `CAP-RWA-02` (bagian cetak) | `RWA-MVP-1` | **Baru** |
| `FE-INP-37` | **Serah Terima Pasien Baru** — checklist, saran sistem, tiga kolom tanda tangan | Bagian `FE-INP-35` | Petugas admisi, CRO, perawat | `CAP-RWA-03` | `RWA-MVP-2` | **Baru** |
| `FE-INP-38` | **Gelang & Label Pasien** — pratinjau, cetak, cetak ulang beralasan | Bagian `FE-INP-35` | Petugas admisi, perawat | `CAP-RWA-05` | `RWA-MVP-1` | **Baru** |
| `FE-INP-39` | **Permintaan Privasi** — tombol bahasa Indonesia/English | Bagian `FE-INP-35` | Petugas admisi, kepala ruangan | `CAP-RWA-06` | `RWA-MVP-2` | **Baru** |
| `FE-INP-40` | **IPD** — Data Dasar Rawat Inap, baca dan cetak | Bagian `FE-INP-35` | Petugas admisi | `CAP-RWA-07` | `RWA-MVP-1` | **Baru** |
| `FE-INP-41` | **Pelunasan Deposit** | Bagian `FE-INP-35` | Petugas admisi, kasir | `CAP-RWA-08` | `RWA-MVP-2` | **Baru** |
| `FE-INP-42` | **Estimasi Biaya** — tab Rekap; tab Rinci berisi pemberitahuan (ditunda) | Bagian `FE-INP-35` | Petugas admisi | `CAP-RWA-09` | **Di luar gelombang** (`DEC-INP-020`) | **Baru** — menu tidak ditampilkan sebelum dikirim |
| `FE-INP-43` | **Selisih Biaya** | Bagian `FE-INP-35` | Petugas admisi | `CAP-RWA-11` | `RWA-MVP-2` | **Baru** |
| `FE-INP-44` | **Nilai Kepercayaan** | Bagian `FE-INP-35` | Petugas admisi | `CAP-RWA-13` | `RWA-MVP-2` | **Baru** |

Setiap layar dokumen (`FE-INP-37`, `39`, `41` s.d. `44`) punya dua tab yang sama: **Formulir** dan **Riwayat** (versi beserta alasannya, tanda tangan, dan log cetak).

**Layar lama yang berubah**

| ID | Layar | Perubahan | Gelombang |
|---|---|---|---|
| `FE-INP-04` | Detail Episode | (1) Tombol "Workspace PPRI" tepat sesudah Workspace Dokter, hanya bagi pemegang `InpatientAdmissionDocument : Read` (`RWI-DEC-245`); (2) tombol "Cetak Persetujuan" membuka `FE-INP-36` tab Surat Persetujuan dan dijaga `InpatientAdmissionDocument : Read` (`RWI-DEC-246`); (3) tumpukan "Perlu diketahui" menampilkan peringatan dokumen admisi dari `Warnings` | `RWA-MVP-1` |
| `FE-INP-18` | Cetak Persetujuan | Halaman lama `episodes/[id]/consent-print` **dialihkan** ke `FE-INP-36` tab Surat Persetujuan, supaya tautan lama tetap bekerja. Komponen surat 12 butir dipakai ulang di tab itu | `RWA-MVP-1` |
| `FE-INP-03` | Admisi Rawat Inap, langkah 8 cetak persetujuan | Tetap seperti hari ini (G-30), kecuali kop surat: dibaca dari `GET …/admission-workspace/letterhead`, bukan dari nilai yang ditanam (`PPRI-CAP-09` *Repair*) | `RWA-MVP-1` |
| `FE-INP-12` | Pengaturan Rawat Inap | Sebelas isian baru: delapan kode formulir, kota penandatanganan, batas umur gelang bayi, kode singkat rumah sakit pada label | `RWA-MVP-0` |
| `FE-INP-13` | Butir Administrasi Rawat Inap | Saringan dan kolom jenis (Penutupan / Serah Terima Pasien Baru); isian induk sub-butir dan sumber saran pada formulir | `RWA-MVP-0` |

### 14.2 Peta butir menu dan jalan masuk

Butir menu baru: **tidak ada.** Seluruh layar adalah layar anak.

| Layar anak | Jalan masuk | Butir hak akses penjaga |
|---|---|---|
| `FE-INP-35` | Tombol "Workspace PPRI" di `FE-INP-04`; tautan langsung `…/episodes/{id}/admission?section=<menu>&tab=<tab>` (rute `DEV_DISCRETION`, pola `buildInpatientNursingWorkspaceRoute`) | `InpatientAdmissionDocument : Read`; tanpa hak → "Akses Tidak Tersedia" (`RWI-AC-343`) |
| `FE-INP-36` | Menu "General Consent" `FE-INP-35`; tombol "Cetak Persetujuan" `FE-INP-04`; rute lama `consent-print` | `InpatientAdmissionDocument : Read`; tombol Cetak `: Print` |
| `FE-INP-37`, `39`, `41`, `43`, `44` | Menu masing-masing di `FE-INP-35` | `: Read`; tombol per aksi di 14.4 |
| `FE-INP-38` | Menu "Gelang & Label Pasien" | `: Read`; tombol Cetak `: Print` |
| `FE-INP-40` | Menu "IPD" | `: Read`; tombol Cetak `: Print`; tarif kamar `: ViewAmount` |
| `FE-INP-42` | Menu "Estimasi Biaya" — **tidak tampil** sebelum `EPIC-RWA-09` dikirim | `: Read` |

**Urutan navigasi kiri** mengikuti V1 tanpa Assessment Edukasi (`RWI-DEC-226`) dan tanpa MP Benefit (ditunda): General Consent, Serah Terima Pasien, Gelang & Label Pasien, Permintaan Privasi, IPD, Pelunasan Deposit, Estimasi Biaya (setelah dikirim), Selisih Biaya, Nilai Kepercayaan. Pada MVP tampil delapan menu sampai Estimasi Biaya dikirim.

**Keterjangkauan.** Petugas admisi: Daftar Episode atau Census → pasien → "Workspace PPRI" = 2 klik. Perawat yang menerima pasien: Census → pasien → "Workspace PPRI" → Serah Terima = 3 klik.

### 14.3 Keputusan rupa dan wewenang UI

| Hal | Mengikat | `DEV_DISCRETION` |
|---|---|---|
| Kerangka | Empat wilayah template Workspace Keperawatan (`ClinicalPageHeader`, `ClinicalStateBoundary`, `ClinicalWorkspaceShell`, `ClinicalSectionNav`) — brief pemilik | Warna, jarak, ikon |
| Menu dan tab di alamat halaman | `?section=` dan `?tab=` (`FR-RWA-002`) | Nama kunci section |
| General Consent | Dua tab cetak; tidak ada tombol Simpan; lencana "Cetak saja" (`RWI-DEC-233`) | Susunan tab |
| Lencana | Teks atau ikon, tidak hanya warna (`NFR-RWA-14`) | Bentuk lencana |
| Ukuran gelang dan label | Isi cetakan (14.4.4) | Ukuran kertas dan CSS cetak, diuji dengan printer rumah sakit saat UAT (G-37) |
| Bahasa formulir Privasi | Tombol Indonesia/English mengganti label layar; cetakan dwibahasa (`FR-RWA-060`, `127`) | Letak tombol |
| Judul halaman | "Ruang Kerja PPRI" / "Penerimaan Pasien Rawat Inap" sebagai `draft` (`RWI-OQ-121`) | — |
| Tablet | Dapat dipakai di tablet ≥ 10 inci; navigasi kiri dapat dilipat (`NFR-RWA-10`) | Titik patah tata letak |

### 14.4 Skema fitur per layar

Skema mengunci isi dan sumber data. Kolom "Bila kosong atau gagal" ditulis sebagaimana dibaca petugas. Tombol yang tidak berhak **disembunyikan**, dan server tetap menolak bila dipanggil langsung.

#### 14.4.1 `FE-INP-35` Workspace PPRI — kerangka

```text
+- Ruang Kerja PPRI ----------------------------- [Ringkasan Kelengkapan] [Kembali ke Detail Episode] [Segarkan] -+
|  Penerimaan Pasien Rawat Inap                                                                                 |
+--------------------------------------------------------------------------------------------------------------+
| Tn. Budi Santoso · RM 00-12-34-56 · L · 45 th · 12 Mar 1981 | RI-261007-0001 · Masuk 07 Okt 08.15            |
| Kelas 2 · Melati 03 / Bed B · DPJP dr. Andika               | Asuransi PT Asuransi Sehat Sentosa · Kartu 7788…|
| Kontak darurat Ny. Rina Santoso (istri) · Alergi: -         | Deposit: kurang Rp 3.000.000 | lihat kasir        |
| Dokumen admisi 2 dari 6 lengkap  [###---]                   | Peringatan (bila ada)                            |
+-----------------------------+--------------------------------------------------------------------------------+
| DOKUMEN ADMISI              |  [Formulir] [Riwayat]                                                          |
| [Cetak saja] General Consent|                                                                                |
| [Menunggu]   Serah Terima   |   isi menu terpilih (14.4.2 s.d. 14.4.8)                                       |
| [Dicetak]    Gelang & Label |                                                                                |
| [-]          Permintaan Priv|                                                                                |
| [Dicetak]    IPD            |                                                                                |
| [Konsep]     Pelunasan Dep. |                                                                                |
| [Belum]      Selisih Biaya  |                                                                                |
| [Lengkap]    Nilai Kepercaya|                                                                                |
+-----------------------------+--------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala halaman | Judul, subjudul, tiga tombol | — | `: Read` | — |
| Header pasien | Identitas, episode, kamar, DPJP, penjamin dan kartu, kontak darurat utama, alergi | `GET …/admission-workspace/summary` → `Header` | `: Read` | Episode atau pasien gagal → seluruh isi diganti "DATA PASIEN TIDAK DAPAT DIMUAT" + [Coba Muat Ulang], **tanpa form** (`FR-RWA-008`); alergi gagal → "Alergi tidak dapat dimuat" |
| Status deposit | "Kurang Rp …", "Cukup", atau "Tidak wajib" | `GET …/summary/amounts` | `: ViewAmount`; tanpa hak → teks "Deposit: lihat kasir" tanpa memanggil endpoint | "Status deposit tidak dapat dimuat"; form lain tetap jalan |
| Kelengkapan | "Dokumen admisi *x* dari *y* lengkap", daftar yang belum | `summary.Completeness` | `: Read` | Sumber gagal → butir "?" dan "Kelengkapan tidak dapat dihitung untuk: …" |
| Navigasi kiri | Delapan menu MVP berlencana | `summary.Menus` | `: Read` | Lencana "?" |
| Episode tidak tersedia | — | `summary.Availability` | — | `NotYetAdmitted` → "Admisi belum dikonfirmasi"; `ReadOnly` → banner "Episode sudah ditutup — hanya-baca", tombol tulis disembunyikan |

**Penyegaran.** Layar dokumen yang berstatus Menunggu tanda tangan menyegarkan dokumen dan ringkasan setiap **30 detik** dengan pola `use-inpatient-billing-status.js` (`intervalMs`), supaya tanda tangan petugas lain tampil paling lambat 30 detik (`FR-RWA-035`, G-34). Penyegaran berhenti saat dokumen `Completed` atau layar ditinggalkan.

#### 14.4.2 `FE-INP-37` Serah Terima Pasien Baru

```text
Ceklist Serah Terima Pasien Baru — versi 1                         [Menunggu tanda tangan]
No | Uraian                                   | Sudah | Belum | Keterangan
 1 | SURAT PENGANTAR RAWAT  (saran: dr. Andika, 07-10-2026) |  ●  |       |
 2 | MENGHUBUNGI DOKTER (KHUSUS TINDAKAN)     |  ●    |       |
   |  • HASIL PEMERIKSAAN PENUNJANG — Laboratorium | ● |     |
 …
13 | PASANG GELANG  (saran: gelang dicetak 09.50 oleh Sari) | ● |   |
Catatan: …
Admission: Sari ✓ 10.05 | CRO: [Tandatangani sebagai CRO] | Perawat: tunggu pasien menempati bed
[Simpan Konsep] [Kunci & Minta Tanda Tangan] [Buka Kunci] [Cetak] [Buat Versi Koreksi] [Batalkan]
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar butir | Butir beku dokumen, atau butir master beserta saran sebelum dokumen dibuat | `GET …/documents/{id}`; sebelum dibuat `GET …/prefill/NewPatientHandover` | `: Read` | Master kosong → "Butir serah terima belum diatur. Hubungi admin." |
| Pilihan Sudah/Belum dan Keterangan | Saling meniadakan; Belum wajib berketerangan | — | `: Create` / `: Update` | Penolakan kunci menyebut nomor butir |
| Kolom tanda tangan | Admission, CRO, Perawat | `Slots` | `: Sign`, `: SignAsCro`, `: SignAsNurse` | Sebelum dikunci → "Data serah terima belum dikirim oleh petugas admisi" (`RWI-AC-354`); Perawat sebelum pasien di bed → tombol nonaktif "Pasien belum menempati tempat tidur" |
| Tombol | Simpan Konsep, Kunci, Buka Kunci, Cetak, Versi Koreksi, Buang Konsep, Batalkan | `AvailableActions` | `: Create`, `: Update`, `: Print`, `: Cancel` | Tombol yang tidak ada di `AvailableActions` tidak ditampilkan |

Pengguna yang hanya memegang `SignAsCro` atau `SignAsNurse` melihat dokumen **hanya-baca** dengan satu tombol tanda tangan (`FR-RWA-034`).

#### 14.4.3 `FE-INP-36` General Consent — cetak saja

```text
[Surat Persetujuan Pasien Rawat Inap] [Formulir General Consent]                       [Cetak saja]
Hubungan dengan pasien: (•) Istri  ( ) Diri Sendiri ( ) Suami ( ) Anak ( ) Orang Tua ( ) Lainnya
Nama: Rina Santoso         Alamat: Jl. Kenanga No. 5, Bekasi   (dari data relasi pasien)
Tipe kamar: (•) Umum ( ) Khusus (ICU/Isolasi)   Kelas/Kamar: Kelas 2 — Melati 03 (Bed B)
Sudah menerima panduan rawat inap: (•) Ya ( ) Tidak    Keterangan: …
[Cetak]
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Tab Surat Persetujuan | Komponen surat 12 butir yang sudah ada (`inpatient-consent-form.jsx`), kop dari profil rumah sakit | `GET …/general-consent/print-data` | `: Read`; Cetak `: Print` | Gagal → "Data cetak tidak dapat dimuat" + Coba Lagi; Cetak nonaktif |
| Tab Formulir V1 | Hubungan, nama dan alamat penanda tangan, tipe kamar, kelas/kamar, panduan, keterangan | Sama; `SignerCandidates`, `RoomType` | `: Read`; Cetak `: Print` | Relasi tidak cocok → isian terbuka dengan keterangan "tidak ditemukan di data wali/kontak darurat" |
| Pilihan penanda tangan | "Istri/Suami" → relasi `Spouse`; "Anak" → `Child`; "Orang Tua" → `Mother`/`Father`; lebih dari satu → petugas memilih; "Lainnya" → daftar relasi dan kontak darurat atau manual (`RWI-DEC-252`) | Sama | — | — |

Layar ini **tidak mengirim satu pun permintaan tulis** (`RWI-AC-347`); pilihan petugas hilang saat halaman ditutup (`RWI-DEC-233`).

**Revisi 13 Oktober 2026 (`RWI-DEC-295`, `PLAN-REPAIR-EPS-008`, `FE-RWI-241`, `FE-RWI-242`).** Paragraf di atas kini hanya berlaku untuk episode yang **belum** punya persetujuan tersimpan. Bila persetujuan sudah ditandatangani digital pada langkah admisi (`RWI-DEC-272`), tab Formulir General Consent berjalan dalam dua mode:

| Mode | Isi | Sumber data | Butir hak akses |
|---|---|---|---|
| Lihat | Jenis penanda tangan, keterangan hubungan, nama, NIK, telepon, alamat, waktu tanda tangan, panduan rawat inap tersimpan, lencana "Versi N", dan "terakhir diubah". Hubungan pada cetakan diturunkan dari data tersimpan | `GET …/consent` | `InpatientEpisode : Read` |
| Edit | Form terisi dari versi aktif + kanvas tanda tangan opsional (wajib bila jenis penanda tangan berganti, `RWI-DEC-296`) + alasan perubahan 10–500 karakter. Simpan selalu dapat ditekan, lalu validasi dan fokus ke isian yang salah | `POST …/admission-workspace/general-consent/revisions` | `InpatientAdmissionDocument : Update`; episode `Admitted`/`DischargePending` |
| Riwayat Perubahan | Satu kotak per versi: status, waktu, pengubah, alasan, dan "label: lama → baru" | `GET …/admission-workspace/general-consent/history` | `InpatientAdmissionDocument : Read` |

#### 14.4.4 `FE-INP-38` Gelang & Label Pasien

```text
Gelang Dewasa                          Label Pasien
[QR] BUDI SANTOSO, Tn.                 [QR] BUDI SANTOSO / <kode RS>
     12 Mar 1981 (45 th)                    12/03/81  L / 45 th
     00-12-34-56                            00-12-34-56   No. Kartu 7788-0012-3456
Dicetak: 1 kali (09.50, Sari)          Dicetak: belum pernah
[Cetak Gelang]                         [Cetak Label]      -> cetak ulang: pilih alasan
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Pratinjau gelang | Jenis dewasa/bayi, nama bersapaan, tanggal lahir dan umur, No. RM, QR; bayi + dua label kecil | `GET …/identity-labels` | `: Print` | Gagal → "Data gelang tidak dapat dimuat", Cetak nonaktif |
| Pratinjau label | Nama / kode RS, tanggal lahir, jenis kelamin / umur, No. RM, No. Kartu bila ada, QR | Sama | `: Print` | Tanpa nomor kartu → baris tidak dicetak |
| Cetak | Mencatat log, lalu membuka dialog cetak peramban (`react-to-print`) | `POST …/print-logs` | `: Print` | Cetak ulang → dialog alasan wajib; log gagal dicatat → cetak dibatalkan dengan pesan |

QR dirender dari `QrPayload` (`qrcode.react`), tidak dari berkas `QrCodePath` (`RWI-DEC-259`).

#### 14.4.5 `FE-INP-40` IPD

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Lembar dua kolom | Tata letak V1 "DATA DASAR RAWAT INAP/ODC" | `GET …/base-data` | `: Read` | `CanPrint = false` → Cetak nonaktif "Cetak ditahan sampai data wajib terbaca lengkap" + Coba Lagi (`FR-RWA-072`) |
| Isian tanpa sumber | Garis kosong; tidak ada isian ketik (`RWI-AC-365`) | `BlankFields` | — | — |
| "Rencana @ Kamar (Rp)" | Angka atau "lihat kasir" | `GET …/base-data/amounts` | `: ViewAmount` | Tanpa hak atau `NotYetAvailable` → "lihat kasir" |
| Cetak | Log, lalu dialog cetak | `POST …/print-logs` | `: Print` | Cetak ulang → alasan |

#### 14.4.6 `FE-INP-41` Pelunasan Deposit

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Data Wali | Sumber data (relasi, kontak darurat, manual), [Ambil Data Wali], [Reset], nama, alamat, telepon | `prefill/DepositSettlementStatement` → `PartyCandidates` | `: Create` / `: Update` | Tidak ada relasi atau kontak → hanya Manual |
| Form pernyataan | Pasien, No. RM, kelas–kamar; kekurangan dan perhitungan; tanggal surat; jatuh tempo (bawaan, batas) | Dokumen; angka dari `GET …/documents/{id}/amounts` atau `summary/amounts` sebelum dibuat | Angka `: ViewAmount` | Kekurangan 0 → "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan.", Simpan nonaktif; Billing gagal → angka tidak tampil, Simpan nonaktif, Coba Lagi |
| Tanda tangan | Yang menyatakan (catatan kertas), Petugas | `Slots` | `: Sign` | — |
| Cetak | Lewat cetak berupiah | `GET …/documents/{id}/amount-print` | `: ViewAmount` + `: Print` | Tanpa `ViewAmount` → tombol Cetak tidak tampil (`RWI-AC-379`) |

#### 14.4.7 `FE-INP-39` Privasi, `FE-INP-43` Selisih Biaya, `FE-INP-44` Nilai Kepercayaan — pola yang sama

Ketiganya memakai pola `FE-INP-37` (Formulir + Riwayat, tombol menurut `AvailableActions`, kolom tanda tangan menurut `Slots`). Isiannya mengikuti PRD Lampiran A.5, A.9, A.11 dengan perubahan Final:

| Layar | Isian khas | Sumber bawaan | Aturan layar |
|---|---|---|---|
| `FE-INP-39` | Tiga baris kerabat, tiga baris permintaan khusus, privasi transportasi, kota, tanggal, nama penanda tangan, keterangan; kolom Pasien/Keluarga dan Kepala Ruangan | `prefill/PrivacyRequest` | Satu nama per baris; tombol bahasa; kolom Kepala Ruangan `: SignAsHeadNurse` |
| `FE-INP-43` | Data pasien **hanya-baca**; subjek pernyataan; data deklarer (nama, alamat, pekerjaan, tipe dan No. ID, HP maks. 13 digit, telepon kantor); kota, tanggal; kolom Deklarer dan Petugas PPRI | `prefill/CostDifferenceStatement` (subjek "diri saya sendiri" mengisi dari data pasien) | Pasien tunai → lencana "Tidak diperlukan", form tidak dapat dibuat; No. ID disamarkan pada daftar |
| `FE-INP-44` | Penanda tangan (nama, tanggal lahir, umur dihitung, jenis kelamin, hubungan, alamat); data pasien dan agama hanya-baca; 1–5 butir hal yang bertentangan | `prefill/BeliefValues` (butir dari dokumen `Completed` episode lalu sebagai konsep) | Blok identitas pasien dirender dari data, bukan gambar (`FR-RWA-113`) |

#### 14.4.8 `FE-INP-42` Estimasi Biaya — di luar gelombang

Tab Rekap: kepala (jenis tindakan, jadwal, dokter, ruang rawat — terisi dari kasus OK aktif bila ada), baris biaya dengan harga dari tarif bagi `ViewAmount`, baris "Tarif belum tersedia" yang wajib diisi manual beralasan atau dihapus, catatan aturan dari kebijakan Billing. Tab Rinci: pemberitahuan bahwa rincian tagihan berjalan dicetak kasir dari modul Billing (PRD bagian 8). Penanda "ada rencana tindakan/operasi" (`: Update`). Layar ini **tidak dibangun** sebelum `DEC-INP-020` turun dan epic masuk gelombang.

### 14.5 Aksi per peran, data, dan penanganan keadaan

**Aksi per peran** diturunkan dari `contracts/permission-audit-matrix.md` 10.1; tombol dijaga `usePermission(resource, action)` **dan** `AvailableActions` dari server.

| Keadaan | Penanganan |
|---|---|
| Memuat | Kerangka header dan navigasi dengan tanda memuat; form tidak tampil sebelum episode dan pasien termuat |
| Kosong | "Belum ada *nama dokumen* untuk episode ini" + tombol Buat (bila `: Create`) |
| Gagal | Pesan server + Coba Lagi; kegagalan data pasien mengganti seluruh isi (`FR-RWA-008`) |
| Data basi | `409 INP-ADM-DOC-004` → "Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda." + tombol Muat Ulang; isian petugas tidak dibuang diam-diam |
| Pengiriman ganda | Satu `Idempotency-Key` dibuat per niat simpan, tanda tangan, dan catat cetak, dipakai ulang saat dikirim ulang; tombol nonaktif selama permintaan berjalan |
| Hanya-baca | Episode `Closed`/`Cancelled` → `isReadOnlyEpisodeStatus`, tombol tulis disembunyikan; cetak ulang meminta alasan |
| Rupiah | Tidak pernah diminta tanpa `ViewAmount`; teks pengganti "lihat kasir" |
| Hak dimuat | `usePermission` sengaja "boleh" sebelum daftar hak termuat (`use-permission.jsx:17-27`); server tetap menolak `403` |

**Komponen cetak.** `KopSurat`, `A4Document`, `signature-section`, `informasi-pasien-surat` (`src/components/features/surat-component`) dipakai ulang. Workspace PPRI **selalu** mengisi props `KopSurat` dari `Letterhead` server dan kota dari dokumen, dan **tidak pernah** mengandalkan nilai bawaan komponen yang menanam identitas rumah sakit client (`kop-surat.jsx:4-12`, `signature-section.jsx:20-31`). Perbaikan nilai bawaan untuk cetakan Final lain adalah issue terpisah G-RWA-02 (`RWI-DEC-247`).

### 14.6 Ketergantungan test

| Test | Bergantung pada |
|---|---|
| Tombol dan "Akses Tidak Tersedia" | Peran tanpa `InpatientAdmissionDocument : Read` di lingkungan uji (`RWI-OQ-124`) |
| Tiga tanda tangan serah terima | Tiga akun berbeda: admisi, CRO, perawat; pasien samaran dengan bed dipesan lalu ditempati |
| Rupiah | Akun dengan dan tanpa `ViewAmount`; kebijakan deposit Billing untuk penjamin dan kelas uji |
| Kop dan kode formulir | Situs rumah sakit `IsMainSite` dan pengaturan terisi (`RWI-OQ-127`) |
| Gelang bayi | Pasien samaran bayi baru lahir dengan ibu |
| IPD | Kunjungan dengan surat pengantar `Issued`, satu yang dibatalkan, dan satu tanpa surat |

### 14.7 Traceability bagian 14

| Layar | Requirement | Keputusan | Acceptance |
|---|---|---|---|
| `FE-INP-35`, `04` | `FR-RWA-001` s.d. `008` | `RWI-DEC-226`, `234`, `245`, `257`, `258` | `RWI-AC-343` s.d. `346`, `378`, `379` |
| `FE-INP-36`, `18`, `03` | `FR-RWA-020` s.d. `022` (cetak) | `RWI-DEC-233`, `246`, `247`, `251`, `252` | `RWI-AC-347` s.d. `349`, `368`, `372`, `373` |
| `FE-INP-37` | `FR-RWA-030` s.d. `035` | `RWI-DEC-239`, `241`, `255`, `262` | `RWI-AC-353`, `354`, `359` s.d. `361`, `376`, `383` |
| `FE-INP-38` | `FR-RWA-050` s.d. `053` | `RWI-DEC-243`, `253`, `259` | `RWI-AC-363`, `364`, `374`, `380` |
| `FE-INP-40` | `FR-RWA-070` s.d. `072` | `RWI-DEC-244`, `254`, `258` | `RWI-AC-365`, `375`, `386`, `387` |
| `FE-INP-39`, `43`, `44` | `FR-RWA-060` s.d. `062`, `100` s.d. `103`, `110` s.d. `113` | `RWI-DEC-238`, `242`, `256` | `RWI-AC-351`, `352`, `357`, `362`, `377` |
| `FE-INP-41` | `FR-RWA-080` s.d. `085` | `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263` | `RWI-AC-366`, `367`, `379`, `381`, `382`, `385` |
| `FE-INP-12`, `13` | `FR-RWA-030`, `050`, `126` | `RWI-DEC-241`, `243`, `247` | `RWI-AC-360`, `361`, `368` |

## 15. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

### 15.1 Authority dan menu reachability

Nama menu, tiga tab, enam label, asal otomatis, tujuan tersedia, kategori manual dan history lengkap adalah USER_CONFIRMED/product-approved melalui DEC-274–294. Bentuk tab/urutan wilayah, warna, ikon, jarak, responsive arrangement adalah **DEV_DISCRETION** memakai design token/base component existing; pilihan visual di sini **PROPOSED** dan bukan persetujuan UI final. Tidak membuka wawancara ulang.

| Tingkat | Induk | Butir/route | Layar | Hak akses |
| --- | --- | --- | --- | --- |
| 0 / subMenu | Sidebar existing | Health Services existing | Grup existing | Permission turun ke leaf |
| 1 / subItems | Health Services | Inpatient Management / Rawat Inap existing | Grup existing | Permission turun ke leaf |
| 2 / pathname | Inpatient Management | Rename Papan Tempat Tidur → Bed Management; `/health-services/inpatient-management/bed-board` | FE-BM-01 (default), tab FE-BM-02/03 | InpatientBedManagement : Read; masing-masing tab tetap hak sendiri |

Route existing dipertahankan, menu kedua tidak dibuat. Registry lintas submodul canonical pada `../02-module-map.md` bagian 9. Deep link detail episode ke tab transfer mengikuti codec/query utility project existing untuk episode identifier, setelah authorization server. Role lama yang hanya Occupancy Read membutuhkan mapping Read baru yang disahkan BM-G03; seeder tidak otomatis grant.

| ID | Layar | Jalan masuk | Permission/action | Sumber |
| --- | --- | --- | --- | --- |
| FE-BM-01 | Monitoring Bed | Menu Bed Management, tab Monitoring Bed | InpatientBedManagement : Read | metadata + monitoring; available-beds untuk pemesanan dengan hak existing |
| FE-BM-02 | Bed Transfer | Tab Bed Transfer; deep link dari detail episode existing | InpatientBedOccupancy : Read/Transfer dan episode Read + scope | transfer-context, available-beds, placements/transfer |
| FE-BM-03 | Bed Usage History | Tab Bed Usage History; pintasan Pilih bed pada Monitoring | InpatientBedManagement : ReadUsageHistory; scope | usage-history; by-episode hanya jika authorized |
| FE-BM-04 | Detail operasional/pembersihan bed (anak) | Klik bed pada FE-BM-01; bukan menu/tab keempat | Read/StartCleaning/CompleteCleaning/VerifyReadiness sesuai aksi | cleaning-attempts, start/complete/verify + monitoring |
| FE-BM-05 | Konfirmasi transfer (anak) | Konfirmasi pada FE-BM-02 setelah awaited refresh | Transfer dan scope kedua bed | current context + versions; mutation key stabil |
| FE-BM-06 | Cancel reservation/master close-reopen (anak existing) | Aksi sah FE-BM-01 atau halaman master bed existing | InpatientBedOccupancy : Update / Bed : Update sesuai aksi | API existing diperketat, alasan + expected version |

Metadata menyatakan tiga tab BA, tab tanpa hak disabled dengan penjelasan 'Tidak memiliki akses' dan tidak memanggil endpoint/data tab itu. HK hanya dapat Monitoring operasional dan anak cleaning, bukan transfer/history pasien. Direct route/query tab tetap AccessDeniedGate, tidak hanya disabled tab.

### 15.2 Skema fitur (susunan PROPOSED)

```text
Bed Management
[Monitoring Bed] [Bed Transfer] [Bed Usage History]
Monitoring: [Unit] [Kamar] [Status] [Cari bed] [Muat ulang]
            [6 ringkasan status] [Daftar bed + badge + aksi sah]
            -> Detail bed: [timeline cleaning] [mulai/selesai/sahkan/tolak]
Transfer:   [Pasien terotorisasi] [Kamar/bed asal otomatis, readonly]
            [Kamar/bed tujuan tersedia] [Kategori manual] [Alasan]
            -> Konfirmasi setelah data baru selesai dibaca
History:    [Bed wajib] [Kamar opsional] [Periode]
            [Segmen hunian initial/transfer/current + versi koreksi] [Pagination]
```

| Layar | Wilayah | Endpoint/data | Hak tombol/guard | Loading/empty/error/filled |
| --- | --- | --- | --- | --- |
| FE-BM-01 | Header/tiga tab/filter/counts | metadata + monitoring | Read; tab allowed server | Memuat: skeleton; kosong: 'Tidak ada bed sesuai filter'; gagal: 'Data bed gagal dimuat. Muat ulang.' |
| FE-BM-01 | Daftar bed / konflik | monitoring BedOperationalResponse | Aksi hanya AvailableActions ∩ permission current; HK tanpa patient UI | Terisi: enam label, subphase menunggu verifikasi, conflict alert; tidak tampilkan identitas jika HoldingContext omitted |
| FE-BM-01/06 | Pesan/tempatkan/cancel | available-beds + reserve/place/cancel existing | Create/Update, episode+unit scope | Reason wajib cancel; error mempertahankan input; expectedVersion dari fresh result |
| FE-BM-02 | Pilih pasien/asal otomatis | Picker episode existing terotorisasi + transfer-context | Read dan Transfer, episode rights | Kosong: 'Pilih pasien dengan hunian aktif'; gagal/stale: 'Hunian asal berubah. Muat ulang.' |
| FE-BM-02 | Tujuan dan kategori manual | available-beds(episodeId), comparison/allowed categories | Transfer; kedua unit scope | Tidak ada tujuan: 'Tidak ada bed tersedia yang sesuai'; Unknown: 'Acuan kelas belum terverifikasi'; tidak default kategori |
| FE-BM-02/05 | Alasan dan konfirmasi | source/dest IDs, versions, category, reason; placements/transfer | Transfer + guard existing DPJP/folio | Loading refresh menonaktifkan confirm; 409 reload; sukses baru toast sesudah commit, asal waiting |
| FE-BM-03 | Filter bed/kamar/periode dan timeline | usage-history | ReadUsageHistory; identity per-row response | Kosong: 'Belum ada penggunaan pada periode ini'; gagal: 'Riwayat gagal dimuat'; akhir NULL='Masih digunakan'; versi koreksi tidak tampak transfer baru |
| FE-BM-04 | Attempt timeline dan aksi HK | cleaning-attempts/start/complete | StartCleaning/CompleteCleaning + assignment | Start menunggu→cleaning; selesai='Menunggu verifikasi'; tidak menawarkan Available |
| FE-BM-04 | Sahkan/tolak kesiapan | readiness-verifications | VerifyReadiness + assignment verifier | Evidence/reference wajib; reject reason wajib; konflik cycle reload; HK tidak melihat tombol verify |
| FE-BM-06 | Close/reopen master dialog | Master existing API melalui service updated | Bed Update/Delete sesuai aksi | Alasan wajib; occupied/reserved conflict; reopen tampil belum diverifikasi, tidak sukses Available |

### 15.3 Reuse setiap elemen dan struktur target

| Elemen | Keputusan | Alasan/penerapan |
| --- | --- | --- |
| Hero, SummaryGrid, StatusBadge | Reuse | Judul, enam counts dan label/subphase; warna/token existing DEV_DISCRETION |
| DataFilter, FilterSelect, ResourceFilterSelect, FilterDatePicker | Reuse | Unit/room/bed/periode; opsi server scoped, bukan semua unit |
| DataTable + Pagination | Reuse | History/attempts; server pagination; versi koreksi dan ongoing jelas |
| BaseButton, ConfirmModal, BaseDetailView/Card/SidePanel | Reuse | Aksi/tab/modal/bed detail sesuai accessibility; tidak buat base component baru |
| FormControl, TextField dan BaseEditorField existing | Reuse | Reason/reference/category; props mengikuti katalog actual saat build |
| InformationAlert, ToastStack, AccessDeniedGate/Alert | Reuse | Conflict, belum ada hak, timeout dan hasil belum pasti; jangan success sebelum server |
| inpatient-bed-board feature/hook/actions/service/utils existing | Extend / repair | Tab container, DTO six-status, scope actions, await refetch dan stable key |
| bed transfer form existing episode detail | Adapter | Satu shared feature component FE-BM-02; detail mengarahkan ke tab atau memakai komponen yang sama; satu handler validasi |
| Fitur tab/history/cleaning khusus Bed Management | Baru, pada feature layer | Komposisi base yang sudah ada; tidak component library/domain store kedua |
| Redux auth/permission existing + InstanceAxios | Reuse | Pola Next App Router JS/JSX, hooks/service; state filter lokal/URL, Redux hanya shared project state |

```text
QuilvianSystemFrontendDev/src/
├─ app/health-services/inpatient-management/bed-board/page.jsx [Diperbarui; thin route]
├─ components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx [Diperbarui]
├─ components/features/health-services/inpatient-management/
│  ├─ inpatient-bed-board.jsx [Diperbarui]
│  ├─ inpatient-bed-transfer.jsx [Baru; adapter shared form existing]
│  ├─ inpatient-bed-usage-history.jsx [Baru]
│  └─ inpatient-bed-cleaning-detail.jsx [Baru]
├─ lib/hooks/health-services/inpatient-management/
│  ├─ use-inpatient-bed-board.jsx [Diperbarui]
│  ├─ use-inpatient-bed-board-actions.jsx [Diperbarui]
│  ├─ use-inpatient-bed-transfer.jsx [Baru]
│  ├─ use-inpatient-bed-usage-history.jsx [Baru]
│  ├─ use-inpatient-bed-cleaning.jsx [Baru]
│  └─ use-inpatient-episode-detail.jsx [Diperbarui; shared transfer adapter]
├─ lib/services/health-services/inpatient-management/
│  ├─ bed-occupancy.service.js [Diperbarui]
│  └─ bed-management.service.js [Baru; inpatient-api.service.js/InstanceAxios existing]
├─ utils/health-services/inpatient-management/inpatient-bed-utils.jsx [Diperbarui]
└─ utils/menu-sidebar/menu-items.jsx [Diperbarui; rename satu leaf]
```

Dashboard constants, master bed services/views dan admisi/record-departure/correction consumers existing ikut inventaris compatibility; update hanya yang memanggil mutation terdampak saat delivery. Tidak menambah Redux slice bed tanpa kebutuhan shared state yang terbukti; local hook state/URL tab/filter sudah cukup, Redux auth/permission project tetap digunakan. Seluruh file target JS/JSX, tidak TypeScript. Service FE memakai `/v1/...` mengikuti helper/baseURL existing; jangan menambah `/api` dua kali.

### 15.4 Data freshness, double submit dan gangguan

Confirm membuka sesudah **await** refetch context + destination selesai, menggunakan returned result yang sama untuk state dan payload; jangan menaikkan refresh token lalu membaca closure lama (F07). Batalkan/abaikan response lama memakai request sequence/AbortController pattern existing. Simpan tetap server recheck expected versions; belum memilih tujuan/kategori atau refetch pending → confirm disabled.

Hook membuat satu idempotency key untuk satu intent/payload; double-click disabled dan retry key sama. Setelah timeout tampil 'Hasil penyimpanan belum diketahui. Sedang memeriksa hasil', lakukan own outcome lookup; 404 bukan kepastian belum commit. Tidak optimistic success atau mutation replay memakai key baru. Setelah error eksplisit yang memastikan rollback dan payload diperbaiki, intent baru boleh key baru. Setelah success/replay refresh monitoring, available-beds, transfer-context, usage-history dan episode detail yang terdampak.

Clear state/cache sensitif saat actor/unit berubah. Mask dari server tidak diisi kembali dari cache episode lain. Counts mengikuti scope/filter response; countdown hanya petunjuk, bukan local expiry. Error menjaga alasan/kategori agar petugas bisa memperbaiki, menampilkan kode semantik dengan pesan Indonesia; no cached overwrite/offline queue.

### 15.5 Accessibility dan verification

Gunakan role tablist/tab/tabpanel, aria-selected, keyboard navigation/focus return modal dan label form. Badge berisi teks, tidak mengandalkan warna. Small screen mempertahankan pilihan tab/filter dan akses aksi. Uji permission direct API/UI, kontrak PHI-free HK, refresh race, keyboard dan current ongoing history sesuai acceptance matrix. Tidak mengunci library baru atau final visual pada blueprint.

### 15.6 Konsumen admisi transfer IGD — finalisasi 11 Oktober 2026

`src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js` berstatus **Diperbarui** pada target: helper `admitFromTransfer` meneruskan ExpectedBedVersion dan Idempotency-Key stabil sesuai API13.8. Helper tersedia, tetapi pencarian source terbaru tidak menemukan pemanggilnya di UI; jangan mengklaim alur UI sudah terhubung. Pemanggil yang nanti dihubungkan memakai hasil available-beds terbaru dan penanganan uncertain outcome yang sama pada15.4. Satu hasil wrapper tidak boleh membuat episode kedua saat retry. Ini inventaris kompatibilitas konsumen, bukan menu/layar baru.


## Amandemen EPS-006 - 12 Oktober 2026

Cabang transfer: URL menyimpan patientId, transferId, sourceType dan episodeId hasil. Dokter hanya memvalidasi form; booking memanggil admitFromTransfer sekali. Guard pasien/sumber dibaca ulang, draft dan unknown intent disimpan per owner/sumber. Jalur pasien baru/lama/kamar pulih mempertahankan default existing. Perpindahan langkah transfer pada route yang sama memakai History API terintegrasi useSearchParams, tanpa meminta transisi server hanya untuk query langkah; lihat [dokumentasi Next.js](https://nextjs.org/docs/app/getting-started/linking-and-navigating#native-history-api).

[Acuan implementasi EPS-006 versi 1.0](contracts/admission-transfer-repair-006.md) berlaku untuk slice transfer dan menggantikan uraian historis yang bertentangan. Batas runtime dan migration dicatat dalam [laporan verifikasi](task/report/frontend/FIX-EPS-006-08.md).


## Amandemen PLAN-REPAIR-DOK-006 — 2026-10-13

Alur admisi tidak membaca kelas dari hook pembayaran. `resolveInpatientAdmissionRoom` memvalidasi kelas/unit bed; `doctor.submit({selectedBed})` baru menulis sesudah bed dipilih; `bed.prepareBooking` menahan klik ganda dan kegagalan. Episode ID disimpan pada URL untuk resume. Konfirmasi tidak mengedit kelas; transfer memakai kelas target bed, deposit memakai kelas episode server bila sudah tersedia.

Rujukan normatif delta yang disetujui: [RWI-KELAS-006-1.0](../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`. Bagian lama yang bertentangan pada lingkup delta ini digantikan oleh amandemen; bagian lain tetap berlaku.

## Amandemen admisi bayi -> RWI-BAYI-016-1.0, 14 Oktober 2026

Entry new/existing kategori bayi memakai komposisi domain khusus dengan hook/DTO yang sama. View -> hook domain -> Axios service -> util/constants tetap pola existing. Entry transfer/kamar pulih tidak didesain ulang oleh scope ini. Konteks bayi/payer terkunci setelah Draft dan koreksi harus berwenang.

Detail dan batas mengikuti [kontrak admisi bayi](contracts/newborn-admission-repair-016.md). Implementasi/source serta bukti dicatat pada laporan BE-RWI-231 dan FE-RWI-245; packaging/DB runtime belum dinyatakan siap.

## 16. Amandemen Integrasi Denah ↔ Bed Management — 15 Oktober 2026

**Status: `approved` — Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`). Amandemen Integrasi Denah ↔ Bed Management, ditulis 15 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.13.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-299 dan aturan keras RWI-DEC-325 (16.7); keamanan/privasi: OPEN. `approved_by: Muhammad Hamzah`, `approved_at: 2026-10-16` (`RWI-DEC-324`).

> **Catatan amandemen `0.18.0` (disetujui 20 Oktober 2026, `RWI-DEC-352`):** sebagian bagian ini digantikan bagian 21 —
> rupa peta pada 16.1, 16.4, dan 16.6, serta aturan keras 16.7 butir 3. Daftar persisnya ada di 21.1.

Masukan: decision log revision **50**, RWI-DEC-298–323 dan RWI-AC-436–461; gate revision **1.13**, **DNH-RCG-20261015-01**; `DOMAIN_ARCHITECTURE_NOT_RUN`. As-is: impact scan **BM-IMP-20261015-01** (`DNH-CAP-34` s.d. `41`). Snapshot BE `ca09cae909597442385ce4513be9c803549be10e`, FE `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028`. Endpoint yang dipakai ada di API contract 14.

### 16.1 Hierarki wewenang dan apa yang dikunci

| Tingkat | Yang dikunci | Sumber |
| --- | --- | --- |
| Keamanan/privasi/invariant | Penghuni disamarkan menurut hak server; Housekeeping tidak melihat nama, No. RM, maupun HD/LD; jenis kelamin tidak ditampilkan di mana pun pada layar bed | RWI-DEC-287, 315 |
| Brief produk yang disetujui | Denah adalah satu-satunya tampilan bed; **desain denah tidak diubah** kecuali permintaan pemilik: kamar penuh tanpa merah dan tetap bisa diklik, outline hijau untuk kamar yang bisa dipilih, pop up kamar memakai kartu pop up yang sama ditambah daftar bed; alur buka denah → pilih lantai → klik kamar → pilih bed | RWI-DEC-299, 303, 316; rencana 6.3 |
| Konvensi project | Hook + service `InstanceAxios`, App Router JS/JSX, struktur route → view → hook → service. **Base component dan references `build-module-frontend` tidak wajib** untuk Bed Management/denah | `AGENTS.md`/`CLAUDE.md` frontend; RWI-DEC-325 |
| `DEV_DISCRETION` | Letak tombol Impor, bentuk layar Impor (halaman anak atau modal), letak panel HD/LD di Workspace Keperawatan, warna kelompok kelas dari jenis kelas, susunan kontrol editor master di dalam pop up, serta bentuk komponen dan styling Bed Management dengan prioritas tampilan yang sempurna | Didelegasikan RWI-DEC-299 dan RWI-DEC-325 sebatas tidak mengubah tampilan peta |

Warna kelas pada peta saat ini berasal dari `room-classes.js` dummy yang memakai nama kelas lama. Sesudah integrasi, warna kelompok kelas diturunkan dari jenis kelas (`PatientClassType`) dan nama kelas resmi, dengan palet yang sama seperti sekarang. Bila suatu kelas resmi tidak punya padanan warna lama, warnanya `DEV_DISCRETION` dan dicatat pada laporan task.

### 16.2 Peta butir menu dan layar

Tidak ada butir menu baru. Peta lintas sub-modul tetap di [02-module-map.md bagian 10](../02-module-map.md).

| Tingkat | Induk | Butir / route | Layar | Hak akses yang menjaga |
| --- | --- | --- | --- | --- |
| 2 / pathname | Inpatient Management | Bed Management, `/health-services/inpatient-management/bed-board` (existing) | FE-BM-01 Monitoring Bed (kini denah), FE-BM-02, FE-BM-03 | InpatientBedManagement : Read |
| 2 / pathname | Master Data (Health Services) | Master Bed `healthServicesBed`, `/health-services/master-data/bed` (existing) | FE-DNH-03 editor denah | Leaf existing; editor juga butuh Room : Read |
| — | — | `/health-services/inpatient-management/floor-plan` (halaman dummy, tidak pernah di menu) | Dihapus | — |

| ID | Layar | Jalan masuk | Hak akses | Sumber data |
| --- | --- | --- | --- | --- |
| FE-DNH-01 | Komponen denah bersama (bukan layar) | Dipakai FE-BM-01, FE-BM-02, langkah Pilih Bed admisi dan admisi bayi, panel transfer Workspace Keperawatan, detail episode, FE-DNH-03 | Mengikuti layar pemakai | Geometri konstanta frontend + endpoint denah layar pemakai |
| FE-DNH-02 | Pop up kamar (anak), mode Lihat, Pilih, dan Master | Klik kamar pada FE-DNH-01 | Aksi per bed mengikuti `AvailableActions` ∩ permission | Data kamar dari respons denah yang sama; mutasi lewat endpoint existing |
| FE-DNH-03 | Master Bed — editor denah | Butir menu Master Bed | Bed : Read + Room : Read; aksi: Room : Update/Create, Bed : Create/Update/Delete | `GET rooms/floor-plan-layout`; writer master existing; `POST rooms/floor-plan-layout/rooms` |
| FE-DNH-04 | Impor susunan denah (anak) | Tombol "Impor susunan denah" pada FE-DNH-03, hanya bila Room : ImportFloorPlan | Room : ImportFloorPlan | Geometri dan data kamar denah frontend + `POST rooms/floor-plan-layout/import` |
| FE-DNH-05 | Panel High/Low Dependency (anak) | Workspace Keperawatan (ringkasan episode) dan detail episode; isian catatan awal pada langkah Dokter admisi | Baca: InpatientEpisode : Read. Ubah: InpatientEpisode : SetCareDependency + aturan pencatat server | `GET episodes/{id}`, `PATCH .../care-dependency`, `GET .../care-dependency-history` |

Layar existing yang berubah: FE-BM-01 (kartu dan tabel diganti denah, RWI-DEC-320), FE-BM-02 (tujuan transfer dipilih lewat denah), langkah 5 Pilih Bed admisi termasuk admisi bayi, panel transfer Workspace Keperawatan, dan transfer pada detail episode. Halaman detail/tambah/ubah bed existing (`master-data/bed/[slug]`, `create`) dipertahankan hanya untuk bed di kamar tanpa kunci denah, dengan jalan masuk dari daftar "Kamar tanpa posisi di denah" pada FE-DNH-03.

### 16.3 Skema fitur per layar

Pop up kamar (FE-DNH-02), dari rencana 6.3:

```text
┌──────────────────────────────────────────────────────────────┐
│                           Kelas 2 329                     ✕  │
│                      Lantai 3 · Barat Laut                   │
│     ● Kelas 2 | Tersedia | High Dependency (1 HD · 1 LD)     │
│ ──────────────────────────────────────────────────────────── │
│ 329-A ● Terisi    Tn. Budi S. · RM 00-12-34 · High Dep.      │
│ 329-B ● Terisi    Ny. Sari W. · RM 00-56-78 · Low Dep.       │
│ 329-C ● Tersedia                     [ Pilih bed ini ]       │
│ Mode Pilih: pembersihan bersyarat; pilih hanya bila layak    │
│ Mode Lihat: aksi pembersihan/tutup-buka/riwayat per bed      │
│ Mode Master: [Kelas] [Arah wing] [Tambah bed] [Tambah boks]  │
└──────────────────────────────────────────────────────────────┘
```

Editor Master Bed (FE-DNH-03) dan Impor (FE-DNH-04):

```text
Master Bed
[Lantai 2][3][4][5][8][9]   [Impor susunan denah]*   [Kamar tanpa posisi di denah (n)]
[Peta lantai — desain sama; ruang cadangan abu-abu hanya di sini]
  klik kamar   -> pop up mode Master
  klik cadangan -> [Nomor] [Kelas] [Unit] [Arah] [Jumlah bed] [Jumlah boks] [Aktifkan]

Impor susunan denah
[Versi data denah] [Lingkungan + akibatnya pada kesiapan bed]
[Tabel kamar: lantai | nomor | nama | kelas | unit (pilih) | arah | bed | boks | kamar existing (opsional)]
[Isi unit satu lantai sekaligus]  [Periksa]  -> [Ringkasan + alasan per kamar]  [Impor]
```

Panel HD/LD (FE-DNH-05):

```text
Tingkat ketergantungan
[High Dependency]  dicatat Ns. A · 15-10-2026 09:10 · Penilaian perawat
(•) High  ( ) Low   [Catatan]   [Simpan]      [Lihat riwayat]
```

| Layar | Wilayah | Data / endpoint | Tombol dan hak | Kosong / gagal |
| --- | --- | --- | --- | --- |
| FE-DNH-01 | Tab lantai + peta | `Floors` + `Rooms` respons denah; lantai bawaan dari server atau lantai terakhir di browser | — | Memuat: kerangka peta; gagal: "Denah gagal dimuat. Muat ulang."; lantai tanpa kamar berkunci: "Belum ada kamar pada lantai ini" |
| FE-DNH-01 | Badge kamar dan outline | `Counts`, `RoomStatus`, `IsSelectable` | Klik selalu aktif, termasuk kamar Penuh | Kamar Penuh tanpa merah dan tanpa outline (RWI-DEC-303) |
| FE-DNH-02 | Judul pop up | Nama kamar, lokasi, kelas, status kamar, `DependencySummary` | — | Ringkasan HD/LD tidak tampil bila `DependencySummary` kosong |
| FE-DNH-02 | Baris bed | `Beds`: label, status, penghuni sesuai hak, sisa waktu pesanan, alasan tidak layak | Lihat: start/complete/verify, tutup/buka, riwayat. Pilih: "Pilih bed ini" hanya pada bed layak; `Pembersihan` boleh tampil pada bed terkait hanya dari irisan `AvailableActions` dan permission efektif pengguna. Sesudah tindakan, denah serta kelayakan dimuat ulang; bed tetap tidak dapat dipilih sampai server mengembalikan `Available` dan `Eligibility.IsEligible = true`. Master: ubah label, tutup/buka, kurangi | Penghuni tersamar sesuai server; tidak mengisi ulang identitas dari cache. Contoh: `325-A` `WaitingCleaning` menampilkan `Pembersihan` hanya bila respons memuat `StartCleaning` dan pengguna berhak; sesudah start status `Cleaning` masih belum dapat dipilih |
| FE-DNH-02 | Konfirmasi Pilih | `POST reservations` atau transfer existing, versi bed dari respons terbaru | InpatientBedOccupancy : Create / Transfer | 409: "Bed baru saja dipilih petugas lain. Denah dimuat ulang."; 422: pesan aturan dari server |
| FE-DNH-03 | Daftar kamar tanpa posisi | `RoomsWithoutFloorPlanKey` | Tautan ke halaman bed/kamar existing | Kosong: tidak tampil |
| FE-DNH-03 | Ubah kelas | `PUT rooms/{id}` + alasan + `AffectedBedVersions` | Room : Update; tombol tetap aktif, server menolak bila berpenghuni | 409: "Kelas 2 329 masih dihuni 2 pasien" |
| FE-DNH-03 | Tambah bed / boks, kurangi, ubah label | Writer bed existing | Bed : Create/Update/Delete | 422 riwayat: "329-A punya riwayat, dinonaktifkan saja"; 409 label |
| FE-DNH-03 | Aktifkan ruang cadangan | `POST rooms/floor-plan-layout/rooms` | Room : Create | 409 nomor: "Nomor 324 sudah dipakai" |
| FE-DNH-04 | Tabel impor dan ringkasan | Manifest dari data denah frontend; `POST .../import` `DryRun` true lalu false | Room : ImportFloorPlan | Penolakan per kamar ditampilkan dengan alasan; tombol Impor tetap aktif, kamar yang ditolak tidak ikut |
| FE-DNH-05 | Nilai saat ini + riwayat | Detail episode + riwayat | Simpan: SetCareDependency; server menentukan sumber | 403: "Hanya perawat penanggung jawab atau DPJP yang dapat mengubah"; 409: "Sudah diubah petugas lain. Muat ulang." |

Tombol simpan tidak pernah dinonaktifkan karena isian kosong; klik → validasi → ringkasan kesalahan dan fokus ke isian wajib pertama. Relasi Guid (kelas, unit, kamar existing) dipilih dari daftar berlabel nama + kode; Guid tidak pernah ditampilkan atau diketik.

### 16.4 Reuse elemen dan struktur target

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| `floor-plan-drawing`, `floor-plan-room`, `floor-plan-view`, `zoom-toolbar`, `floor-plan.module.css` | Pindah apa adanya ke feature layer | Satu denah untuk semua layar; tampilan dibuktikan sama lewat perbandingan screenshot per lantai |
| `room-name-dialog.jsx` | Diganti pop up kamar | Kartu, garis aksen, dan baris rincian dipertahankan; daftar bed ditambahkan |
| Geometri `lantai-*.plan.js`, `floors.js`, `plan-primitives.js` | Pindah ke `src/lib/constants/.../floor-plan/`; id gambar dipetakan ke kunci denah netral kelas | `DNH-MIS-06` |
| `lantai-*.occupancy.js`, `room-classes.js` dummy | Dihapus | Kelas, jumlah bed, dan status dari API |
| `bed-monitoring-cards.jsx`, `bed-monitoring-table-columns.jsx`, `inpatient-bed-board.jsx` + CSS-nya | Dihapus | RWI-DEC-299, 320 |
| Endpoint dan service `bed-board` | Dipertahankan | Daftar pasien rawat inap (`DNH-MIS-09`) |
| `use-bed-management`, `use-inpatient-admission-bed`, `use-inpatient-bed-transfer`, `use-bed-master-confirmation`, `bed-master-mutation.service` | Reuse / extend | Aksi pembersihan, pesan, transfer, alasan dan versi bed sudah ada (`DNH-CAP-41`) |
| `ConfirmModal` (prop `show`), `BaseButton`, `Badge` (prop `label`), `InformationAlert`, `ToastStack`, `AccessDeniedGate`, `DataTable`, `FormControl` | Boleh dipakai, **tidak wajib** (RWI-DEC-325) | Komponen tampilan Bed Management boleh dibuat khusus di folder feature denah dengan CSS module sendiri; tidak membuat base component baru di `base-features` |
| Isian dan pesan jenis kelamin di `room-constants.jsx`, `room-utils.jsx`, `bed-constants.jsx`, `bed-utils.jsx`, `inpatient-placement-utils.jsx` | Dihapus | RWI-DEC-315; satu gelombang dengan backend |

```text
QuilvianSystemFrontendDev/src/
├─ app/health-services/inpatient-management/floor-plan/ [Dihapus; halaman dummy]
├─ app/health-services/master-data/bed/bed-client.jsx [Diperbarui; render editor denah]
├─ components/features/health-services/inpatient-management/floor-plan/
│  ├─ floor-plan-drawing.jsx [Dipindah; tanpa perubahan tampilan]
│  ├─ floor-plan-room.jsx [Dipindah; badge, outline hijau, tanpa merah]
│  ├─ floor-plan-view.jsx [Dipindah; terima data API dan mode]
│  ├─ zoom-toolbar.jsx [Dipindah]
│  ├─ floor-plan-room-popup.jsx [Baru; pengganti room-name-dialog]
│  ├─ floor-plan-bed-row.jsx [Baru]
│  └─ floor-plan-master-actions.jsx [Baru]
├─ components/features/health-services/inpatient-management/inpatient-bed-board.jsx [Dihapus]
├─ components/features/health-services/inpatient-management/inpatient-bed-transfer.jsx [Diperbarui; tujuan lewat denah]
├─ components/features/health-services/inpatient-management/care-dependency-panel.jsx [Baru]
├─ components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx [Diperbarui]
├─ components/view/health-services/inpatient-management/bed-management/bed-monitoring-cards.jsx [Dihapus]
├─ components/view/health-services/inpatient-management/bed-management/bed-monitoring-table-columns.jsx [Dihapus]
├─ components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx [Diperbarui]
├─ components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx [Diperbarui]
├─ components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx [Diperbarui; catatan awal HD/LD]
├─ components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx [Diperbarui]
├─ components/view/health-services/inpatient-management/nursing-workspace/sections/transfer/nursing-transfer-form-panel.jsx [Diperbarui]
├─ components/view/health-services/inpatient-management/nursing-workspace/components/nursing-quick-summary-panel.jsx [Diperbarui; panel HD/LD]
├─ components/view/health-services/master-data/bed/master-data-bed-view.jsx [Diganti]
├─ components/view/health-services/master-data/bed/master-data-bed-floor-plan-view.jsx [Baru]
├─ components/view/health-services/master-data/bed/floor-plan-import-view.jsx [Baru]
├─ lib/constants/health-services/inpatient-management/floor-plan/ [Baru; geometri dipindah + peta kunci]
├─ lib/hooks/health-services/inpatient-management/use-floor-plan.js [Baru]
├─ lib/hooks/health-services/inpatient-management/use-care-dependency.js [Baru]
├─ lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx [Dihapus bila konsumen terakhirnya ikut diganti]
├─ lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js [Baru]
├─ lib/hooks/health-services/master-data/bed/use-floor-plan-import.js [Baru]
├─ lib/services/health-services/inpatient-management/bed-management.service.js [Diperbarui]
├─ lib/services/health-services/inpatient-management/bed-occupancy.service.js [Diperbarui]
├─ lib/services/health-services/master-data/floor-plan-layout.service.js [Baru]
├─ lib/constants/health-services/master-data/{room-constants,bed-constants}.jsx [Diperbarui]
├─ utils/health-services/master-data/{room-utils,bed-utils}.jsx [Diperbarui]
└─ utils/health-services/inpatient-management/inpatient-placement-utils.jsx [Diperbarui]
```

Test unit yang mengunci perilaku lama (`inpatient-bed-board.test.mjs`, `inpatient-bed-management.test.mjs`, `inpatient-placement.test.mjs`, dan yang menguji isian jenis kelamin) disesuaikan atau dihapus bersama komponennya (`DNH-CAP-40`). Berkas admisi (`inpatient-admission-*`, `inpatient-newborn-admission-*`, `use-inpatient-admission-bed.jsx`) sedang diubah pekerjaan paralel issue 022; perubahan denah pada berkas itu dikerjakan sesudah pekerjaan tersebut masuk.

### 16.5 Data, kesegaran, dan gangguan

Satu lantai satu panggilan. Ganti lantai membatalkan respons lama (pola request sequence existing). Sesudah aksi apa pun di pop up, data lantai dimuat ulang dan pop up memakai hasil baru; konfirmasi Pilih memakai versi bed dari respons terbaru. Kunci idempotensi dibuat sekali per niat; klik ganda dinonaktifkan dan retry memakai kunci sama (bagian 15.4). Lantai terakhir yang dipakai disimpan di browser sebagai kenyamanan per pengguna, dibungkus try/catch.

Peta menggambar semua bentuk kamar dari geometri, lalu mengisi kamar yang kuncinya ada di respons. Bentuk tanpa data tampil seperti ruang tanpa label di desain sekarang. Ruang cadangan hanya tampil abu-abu di FE-DNH-03.

### 16.6 Aksesibilitas dan verifikasi

Kamar pada peta dapat difokus dengan keyboard dan dibuka dengan Enter; pop up mengembalikan fokus ke kamar saat ditutup. Status bed dan kamar selalu berupa teks, tidak hanya warna. Layar 390 px tidak bergeser horizontal. Bukti tampilan: screenshot setiap lantai sebelum dan sesudah, berbeda hanya pada data (rencana 10.6). Uji privasi memakai profil Housekeeping terhadap respons API, bukan hanya kolom yang disembunyikan.

### 16.7 Aturan keras pemilik — 16 Oktober 2026 (RWI-DEC-325)

> **Aturan keras !!!** Berlaku untuk seluruh task frontend integrasi denah (`FE-RWI-246` s.d. `253`).
>
> 1. References skill `build-module-frontend` — katalog dan gerbang keputusan base component, design
>    token, pola komposisi halaman, standar fitur master data, checklist konsistensi UI, profil proyek,
>    dan kebijakan test — **tidak wajib diikuti**.
> 2. Bed Management **fleksibel**; prioritasnya tampilan yang sempurna dan bagus.
> 3. Tampilan floor plan yang sudah didesain rapi **tidak boleh diubah** (RWI-DEC-299). Yang boleh berubah
>    hanya permintaan pemilik: tanpa garis merah kamar penuh, outline hijau kamar yang bisa dipilih,
>    badge, dan daftar bed di pop up (RWI-DEC-303, 316).
> 4. Base component **tidak wajib** dipakai, karena tampilan Bed Management akan menjadi contoh untuk
>    tampilan lain.

> **Butir 3 digantikan `RWI-DEC-343` (20 Oktober 2026):** atas arahan atasan, rupa denah boleh diubah
> menjadi peta bangsal (bagian 21). Butir 1, 2, dan 4 tetap berlaku.

Aturan ini adalah brief UI pemilik, sehingga pada hierarki 16.1 ia berada di atas konvensi project dan
`DEV_DISCRETION`. Yang tetap mengikat karena berada di atasnya atau di luar soal tampilan: privasi
penghuni (RWI-DEC-287), tanpa jenis kelamin (RWI-DEC-315), Guid tidak ditampilkan maupun diketik,
tombol simpan tidak dinonaktifkan karena isian kosong, kontrak API `0.13.0`, stack Next.js App Router
JavaScript/JSX dengan Redux dan Axios `InstanceAxios`, struktur route → view → hook → service, serta
`npm run lint` dan `npm run build`.

**Contoh.** Pop up kamar boleh dibangun sebagai komponen khusus denah dengan CSS module sendiri, tanpa
`ConfirmModal` atau `BaseButton`, selama kartu pop up denah yang sekarang (judul, garis aksen kelas,
baris rincian) tetap terlihat sama. Komponen baru tinggal di folder feature denah. Mempromosikannya
menjadi base component bersama adalah keputusan terpisah, karena `AGENTS.md` frontend melarang membuat
base component baru tanpa permintaan eksplisit.

---

## 17. Amandemen Bagian 17 — Arsitektur Frontend Layar Transfer Pasien Masuk pada Detail Episode & Workspace PPRI

| Field | Nilai |
| --- | --- |
| Kontrak Target | `0.14.0` — `draft` |
| Tanggal | 16 Oktober 2026 |
| Fokus Perubahan | Layar Detail Episode (`InpatientEpisodeDetailView`), Laci Serah Terima Klinis (`TransferHandoverDrawer`), dan Dokumen Admisi Workspace PPRI (`AdmissionWorkspaceClient`) |
| Status | `draft` |

### 17.1 Spesifikasi Layar 1: Detail Episode Rawat Inap (`InpatientEpisodeDetailView`)

1. **Banner Serah Terima Klinis (`TransferHandoverBanner.jsx`)**:
   - **Kondisi Tampil**: Jika kueri `GET /api/v1/health-services/clinical-management/transfer-handovers?episodeId={id}` mengembalikan dokumen berstatus `Sent`.
   - **Komponen**: `InformationAlert` varian `warning` / `accent-amber`.
   - **Teks**: "Terdapat dokumen serah terima pasien masuk dari {SourceUnitName} yang menunggu verifikasi penerimaan ruangan."
   - **Aksi**: Tombol `[Lihat Serah Terima Klinis]` membuka `TransferHandoverDrawer`.
2. **Alert Non-Blocking Dokumen Admisi PPRI**:
   - **Kondisi Tampil**: Jika kueri ringkasan admisi mengembalikan ada dokumen PPRI yang belum `Completed`.
   - **Komponen**: `InformationAlert` varian `info`.
   - **Teks**: "Dokumen admisi di Workspace PPRI sedang diproses ({CompletedCount}/{TotalCount} selesai). Pelayanan dan perawatan medis di bangsal tetap berjalan penuh."
   - **Aksi**: Tautan pintas `[Buka Dokumen PPRI]`.
3. **Laci Serah Terima Klinis (`TransferHandoverDrawer.jsx`)**:
   - **Komponen Eksisting**: Diadaptasi dari `src/components/features/health-services/clinical-management/transfer-handover/TransferHandoverDrawer.jsx`.
   - **Area TTV & Fisik**: Menampilkan kartu ringkas Tanda-Tanda Vital unit asal (TD, Nadi, RR, Suhu, SpO2, Skala Nyeri, Kesadaran).
   - **Area Alat Medis Terpasang**: List badge alat medis aktif (Infus, Kateter, Oksigen, dsb.).
   - **Area Rekonsiliasi Obat & Alergi**:
     - Peringatan Alergi: Tag merah tegas jika pasien memiliki riwayat alergi obat/makanan.
     - Tabel Terapi Obat: Kolom Nama Obat, Dosis, Rute, Jam Terakhir Diberikan di Unit Asal, dan Status Rekonsiliasi (Lanjut / Tunda / Stop).
   - **Area Penetapan Perawat PPJA (Wajib)**:
     - Form Dropdown `BaseSelect`: `Pilih Perawat Penanggung Jawab Asuhan (PPJA) *`.
     - Mengambil daftar perawat aktif di unit ruangan (`GET /api/v1/health-services/inpatient-management/wards/{wardId}/nurses`).
     - Guard Tombol: Tombol `[Terima Pasien]` **terkunci / disabled** bila PPJA belum dipilih.
   - **Aksi Penolakan**:
     - Tombol `[Tolak / Minta Klarifikasi]` membuka dialog input alasan (minimal 10 karakter).

---

### 17.2 Spesifikasi Layar 2: Workspace PPRI (`AdmissionWorkspaceClient`)

1. **Sub-Menu Gelang & Label (`WristbandAndLabelView.jsx`)**:
   - **Tampilan**: Pratinjau visual gelang pasien ranap resmi (No RM, Nama, Tanggal Lahir, Jenis Kelamin, Bed/Ruangan, Barcode/QR).
   - **Tombol**:
     - `[Cetak Gelang Ranap Resmi]` memicu dialog printer thermal gelang.
     - `[Cetak Stiker Label Medis]` memicu dialog printer stiker label (ukuran standar rekam medis/tabung lab).
   - **SOP Petugas**: Gelang sementara IGD dilepas dan digantikan dengan Gelang Ranap resmi ini saat proses pendorongan fisik pasien ke bangsal.
2. **Sub-Menu Formulir Serah Terima Pasien Baru (`HandoverChecklistForm.jsx`)**:
   - **Tampilan**: Checklist 15 butir serah terima berkas admisi.
   - **Aksi Petugas Admisi**: Mengisi status (Sudah/Belum), mengunci dokumen (`[Kunci Dokumen]`), lalu menandatangani kolom Admisi.
   - **Aksi Perawat Ruangan**: Menandatangani kolom Perawat (tombol aktif setelah pasien menempati tempat tidur).
    - **Tombol Cetak**: `[Cetak Berkas Serah Terima Pasien Baru]` (`HandoverChecklistPrint.jsx`).

## 18. Amandemen Frontend Admisi Bayi — `RWI-BAYI-022-2.0.0`

Cabang bayi memakai tujuh slug: `newborn-context`, `newborn-identity`, `doctor`,
`bed-selection`, `consent-form`, `confirmation`, dan `newborn-documents`. State finansial tidak
lagi dimiliki cabang ini.

- Langkah 1 membaca candidate kasus persalinan, bukan pencarian seluruh pasien perempuan.
- Langkah 2 hanya mengirim ibu, kasus, tanggal/jam lahir, jenis kelamin, dan data kelahiran
  opsional. Nama dan urutan tidak dikirim browser; retry memakai `Idempotency-Key` stabil.
- Encounter mengirim payment source ibu beserta versi waktunya dan mengosongkan pilihan payer
  manual.
- Ringkasan lengkap serta informasi penjamin/deposit baca-saja hanya ada di `confirmation`.
- URL lama `payment`/`deposit` pada cabang bayi dinormalisasi ke `bed-selection`; alur umum tetap
  memakai step finansial existing.
- Tombol pembatalan existing tidak diubah oleh amandemen ini.

Bukti source dan validasi: `FE-RWI-257..259`.

---

## 19. Amandemen Frontend Pendaftaran Admisi Pasien ODC (One Day Care) — Kontrak `0.15.0` (`RWI-DEC-327` s.d. `RWI-DEC-332`)

| Field | Nilai |
|---|---|
| Kontrak Target | `0.15.0` — `draft` |
| Tanggal | 16 Oktober 2026 |
| Status | `draft` |
| Masukan Bisnis | `00-interview-decisions.md` revision `53` (`RWI-DEC-327` s.d. `RWI-DEC-332`), `01-existing-capability-map.md` Bagian 21 |
| Fokus Perubahan | Opsi kartu tipe pasien ke-4 ODC di Langkah 2, penyaringan papan bed ODC di Langkah 6, judul & klausul 24 jam cetak General Consent di Langkah 8 |

### 19.1 Alur Wizard & Langkah Admisi ODC

Sesuai `RWI-DEC-327` dan `RWI-DEC-328`, alur pendaftaran admisi pasien ODC menggunakan alur 8 langkah rawat inap reguler tanpa membuat alur khusus yang terpisah. Perbedaan perilaku alur ODC terletak pada:
1. **Langkah 2 (Pilih Jenis Pasien)**: Memilih opsi kartu ke-4 *"Pasien ODC (One Day Care)"*.
2. **Langkah 5 (Pilih Dokter)**: Penetapan DPJP penanggung jawab tindakan ODC bersifat wajib.
3. **Langkah 6 (Pilih Tempat Tidur)**: Papan denah/tempat tidur otomatis menerapkan filter ketat hanya menampilkan bed/kamar bertanda ODC.
4. **Langkah 8 (Persetujuan Pasien)**: Dokumen General Consent mencantumkan judul layanan One Day Care (ODC) dan klausul batas observasi/perawatan maksimal 24 jam.

```text
[Langkah 1: Mode Masuk] ──► [Langkah 2: Tipe Pasien (ODC)] ──► [Langkah 3: Identitas Pasien]
         │
         ▼
[Langkah 4: Pembayaran/Penjamin] ──► [Langkah 5: Dokter (DPJP Wajib)] ──► [Langkah 6: Pilih Bed ODC]
         │
         ▼
[Langkah 7: Konfirmasi Deposit] ──► [Langkah 8: Cetak General Consent ODC & Gelang] ──► Selesai
```

### 19.2 Spesifikasi Komponen & Modifikasi Frontend

1. **Konstanta Alur Admisi (`inpatient-admission-flow-constants.jsx`)**:
   - Lokasi: `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
   - Pada `INPATIENT_PATIENT_TYPE`, tambahkan `ODC: "odc"`.
   - Pada `INPATIENT_PATIENT_TYPE_OPTIONS`, tambahkan kartu opsi ke-4:
     ```javascript
     Object.freeze({
       value: INPATIENT_PATIENT_TYPE.ODC,
       title: "Pasien ODC (One Day Care)",
       description: "Pasien observasi atau tindakan medis singkat terencana dengan estimasi durasi rawat kurang dari 24 jam.",
       icon: "O",
     })
     ```
   - Fungsi `resolvePatientTypeContinuationSlug` (`:351`) secara otomatis menangani nilai `ODC` untuk mengarahkan ke langkah berikutnya (`registration` untuk Pasien Baru, `patient-search` untuk Pasien Lama).

2. **Render Kartu Pilihan Jenis Pasien (`inpatient-admission-view.jsx` & `inpatient-admission-visit-category-step.jsx`)**:
   - Kedua layar ini merender `INPATIENT_PATIENT_TYPE_OPTIONS.map(...)` menggunakan `BaseCheckboxCard`.
   - Penambahan opsi pada konstanta langsung memunculkan kartu ke-4 dengan rapi dan responsif tanpa perlu pengubahan JSX komponen render kartu.

3. **Penyuntikan Filter Papan Ketersediaan Bed (`useInpatientAdmissionBed.jsx` & `inpatient-bed-utils.jsx`)**:
   - Lokasi: `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` dan `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx`
   - Pada `buildBedBoardQuery` dan `buildAvailableBedQuery` di `inpatient-bed-utils.jsx`:
     ```javascript
     if (patientContext?.isOdcBed) {
       query.isOdcBed = true;
     }
     ```
   - Pada `inpatient-admission-view.jsx:757`, teruskan konteks ODC ke `useInpatientAdmissionBed`:
     ```javascript
     const bed = useInpatientAdmissionBed({
       episodeId: doctor.episodeId,
       active: flow.currentStep?.slug === INPATIENT_BED_STEP_SLUGS.SELECTION || flow.currentStep?.slug === INPATIENT_BED_STEP_SLUGS.BOOKING,
       adoptExistingReservation: resume.isReady,
       patientContext: {
         patientId: activePatientId,
         isOdcBed: flow.patientType === INPATIENT_PATIENT_TYPE.ODC,
         // context lainnya...
       },
       // ...
     });
     ```
   - Dengan parameter ini, API backend hanya mengembalikan tempat tidur dengan `IsOdcBed == true` dan menyembunyikan bed rawat inap reguler (`RWI-DEC-327`).

4. **Formulir & Lembar Cetak General Consent ODC (`inpatient-consent-print-view.jsx`)**:
   - Lokasi: `src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx`
   - Judul dokumen dibuat adaptif berdasarkan `flow.patientType` atau indikator ODC episode:
     - Jika pasien adalah ODC: Judul dokumen berubah menjadi *"SURAT PERSETUJUAN PASIEN ONE DAY CARE (ODC)"* dan subjudul *"General Consent — Pelayanan One Day Care (ODC)"*.
     - Klausul Tambahan Khusus ODC: Pada bagian pernyataan persetujuan, dicantumkan klausul eksplisit:  
       > *"Pasien dan keluarga memahami bahwa pelayanan One Day Care (ODC) merupakan tindakan/observasi terencana dengan batas waktu perawatan maksimal 24 jam. Apabila kondisi klinis pasien memerlukan perawatan lanjutan melampaui 24 jam, pasien akan dialihkan ke perawatan rawat inap reguler sesuai ketentuan rumah sakit."*
   - Pencocokan kamar: `matchRoomOptionKey` (`inpatient-consent-utils.jsx:51`) sudah mengenali kata kunci `"odc"` dan secara otomatis mencentang pilihan kelas ODC pada lembar persetujuan.

5. **Eskalasi Rawat > 24 Jam via Transfer Bed Management (`bed-management-view.jsx`)**:
   - Sesuai `RWI-DEC-331`, eskalasi rawat inap pasien ODC yang dirawat lebih dari 24 jam dilakukan langsung oleh perawat/petugas melalui layar **Bed Management** existing (menu mutasi *Transfer Bed*).
   - Petugas memilih kamar dan tempat tidur rawat inap reguler tujuan, menginput alasan mutasi ("Eskalasi perawatan ODC ke rawat inap reguler > 24 jam"), dan menekan tombol konfirmasi transfer.
   - Episode lama tetap berjalan dan ID episode tidak berubah. Tidak diperlukan pembatalan atau pendaftaran admisi ulang.

### 19.3 Pohon Folder dan Status File Frontend

```text
QuilvianSystemFrontendDev/src/
├── components/view/health-services/inpatient-management/
│   ├── inpatient-admission-view.jsx                       [Diperbarui; inject isOdcBed ke useInpatientAdmissionBed]
│   ├── inpatient-admission-visit-category-step.jsx        [Ready to reuse; render kartu ODC otomatis]
│   ├── inpatient-consent-print-view.jsx                   [Diperbarui; judul dinamis ODC & klausul 24 jam]
│   └── bed-management/bed-management-view.jsx             [Ready to reuse; transfer eskalasi ODC]
├── lib/constants/health-services/inpatient-management/
│   └── inpatient-admission-flow-constants.jsx             [Diperbarui; tambahkan INPATIENT_PATIENT_TYPE.ODC]
├── lib/hooks/health-services/inpatient-management/
│   ├── use-inpatient-admission-flow.jsx                   [Ready to reuse; kelola state patientType]
│   └── use-inpatient-admission-bed.jsx                    [Diperbarui; teruskan patientContext.isOdcBed]
└── utils/health-services/inpatient-management/
    ├── inpatient-bed-utils.jsx                            [Diperbarui; query builder sertakan isOdcBed]
    └── inpatient-consent-utils.jsx                        [Ready to reuse; matchRoomOptionKey sudah ada 'odc']
```

---

## 20. Amandemen Frontend Lencana Tier Membership & Prioritas Antrean Transfer — `contract_version 0.17.0` (`draft`, `RWI-DEC-338` s.d. `341`)

### 20.1 Tujuan dan Boundary Desain UI

Amandemen ini mengatur visualisasi lencana prioritas klinis (`CITO`) dan hak keanggotaan (`Membership Tier`) pada antarmuka pengguna rawat inap, khususnya:
1. **Layar Tabel Antrean Transfer Pasien (`InpatientAdmissionTransferList.jsx`)**: Mengurutkan pasien secara otomatis dari backend (CITO → Member Prioritas → Reguler) dan menyematkan lencana visual penanda prioritas pada baris pasien.
2. **Layar Langkah Admisi PPRI Transfer (`InpatientAdmissionTransferReviewStep.jsx`)**: Menyajikan lencana CITO dan Tier Membership pada header ringkasan pasien saat petugas admisi sedang memproses admisi dan memilih tempat tidur.

### 20.2 Spesifikasi Komponen & Aturan Visual Lencana

Sesuai `rules/frontend/refine-menu-ui` dan konvensi base component Quilvian:
- Lencana dirender menggunakan komponen `StatusBadge` atau `Badge` standar tanpa hardcoded custom styling CSS.
- **Lencana CITO**:
  - `variant="danger"` (warna merah tegas / solid red).
  - Label: `"CITO"` (kapital).
  - Ditampilkan jika properti `isCito === true`.
- **Lencana Tier Membership**:
  - `variant="warning"` (untuk Tier Gold / Platinum) atau `variant="primary"` (untuk Tier Silver).
  - Label: Nama tier yang diterima dari backend, misalnya `"Platinum"`, `"Gold"`, `"Silver"`.
  - Ditampilkan hanya jika `isPriorityMember === true && Boolean(memberTierName)`.
  - Pasien non-member atau member kadaluwarsa tidak memunculkan lencana membership.

### 20.3 Modifikasi Layar 1: Tabel Antrean Transfer (`InpatientAdmissionTransferList.jsx`)

1. **Kolom Pasien / Prioritas**:
   - Di samping Nomor Rekam Medis (RM) dan Nama Pasien, ditambahkan container flex untuk badge:
     ```jsx
     <div className="flex items-center gap-1.5 flex-wrap">
       {item.isCito && (
         <StatusBadge variant="danger" size="sm">
           CITO
         </StatusBadge>
       )}
       {item.isPriorityMember && item.memberTierName && (
         <StatusBadge variant="warning" size="sm">
           {item.memberTierName}
         </StatusBadge>
       )}
     </div>
     ```
2. **Perilaku Tabel**:
   - Data tabel otomatis tersaji dalam urutan: CITO di posisi teratas, disusul Member Prioritas sesuai level, lalu Reguler.
   - Paging dan pencarian tabel tetap stabil karena sorting ditegakkan secara deterministik di server.

### 20.4 Modifikasi Layar 2: Header Langkah Admisi PPRI (`InpatientAdmissionTransferReviewStep.jsx`)

1. **Header Ringkasan Pasien**:
   - Pada panel ringkasan identitas pasien (sebelum tombol *Pilih Kamar & Bed*), disajikan lencana CITO dan Tier Membership berdampingan dengan Nama, No RM, dan Asal Unit (IGD).
   - Petugas admisi langsung mengetahui bahwa pasien ini merupakan pasien gawat darurat atau pemegang keanggotaan prioritas rumah sakit.
2. **Pemilihan Kamar Tetap Manual**:
   - Tombol *[Pilih Kamar & Tempat Tidur]* tetap membuka modal Denah Bed Management existing.
   - Petugas admisi mencocokkan ketersediaan kamar secara sadar dan profesional tanpa ada auto-booking diam-diam dari sistem (`RWI-DEC-338`).

### 20.5 Invariant Perlindungan Privasi Pasien (`RWI-DEC-341`, `RWI-AC-488`)

- **Denah Bed Management Bangsal Umum Bebas Lencana Tier**:
  - Komponen denah lantai dan pop up kamar (`floor-plan-room-popup.jsx`, `floor-plan-bed-row.jsx`, `bed-management-view.jsx`) **dilarang menampilkan lencana tier membership pasien yang sedang dirawat**.
  - Hal ini menjamin privasi pasien dan mencegah potensi kesenjangan sosial atau persepsi diskriminasi di antara keluarga pasien yang saling melihat papan kamar di bangsal rawat inap umum.

### 20.6 Pohon Folder dan Status File Frontend

```text
QuilvianSystemFrontendDev/src/
└── components/view/health-services/inpatient-management/admission-transfers/
    ├── inpatient-admission-transfer-list.jsx            [Diperbarui; render lencana CITO & Member Tier di kolom pasien]
    └── inpatient-admission-transfer-review-step.jsx     [Diperbarui; render lencana CITO & Member Tier di header ringkasan]
```

---

## 21. Amandemen Peta Bangsal Denah — `contract_version 0.18.0` (`draft`, `RWI-DEC-343` s.d. `RWI-DEC-351`)

**Status: `approved_design_planned` — disetujui Muhammad Hamzah 20 Oktober 2026 (`RWI-DEC-352`);
delivery `PETA-DEL-20261020-01`.** Set kontrak mengikuti [manifest sub-modul](./blueprint-manifest.md)
bagian 23–24; `last_changed_in: 0.18.0`. Owner
produk/domain: Muhammad Hamzah (`RWI-DEC-061`). Arahan perubahan datang dari atasan pengguna dan
dicatat atas nama Muhammad Hamzah (`RWI-DEC-343`). Frontend: pengembang dalam batas bagian 21.3.
Keamanan/privasi: `OPEN`, tidak berubah. `approved_by: Muhammad Hamzah`, `approved_at: 2026-10-20`.

| Hal | Nilai |
| --- | --- |
| Tujuan | Denah Bed Management tidak lagi terlihat seperti gambar kerja arsitek, tetapi menjadi peta bangsal yang bersih dan mudah dibaca, dengan kamar, bed, data, dan alur kerja yang sama |
| Masukan keputusan | [Decision log](../00-interview-decisions.md) revision 62–63: persetujuan `RWI-DEC-352`; palet titik `RWI-DEC-353` dan `RWI-AC-508`; `RWI-DEC-343` s.d. `RWI-DEC-349`, asumsi `RWI-DEC-350` dan `RWI-DEC-351`, `RWI-AC-498` s.d. `RWI-AC-507`, konflik `RWI-CON-018` (ditutup), `RWI-FE-009` s.d. `RWI-FE-012` |
| Kesiapan requirement | Gate revision 1.13 `DNH-RCG-20261015-01`: `DNH-CG-01` (tampilan denah dan status kamar) dan `DNH-CG-05` (HD/LD) `READY_FOR_DOMAIN_DESIGN`. Amandemen ini hanya mengubah rupa `DNH-CG-01`; tidak ada kemampuan, data, status, atau aturan bisnis baru, sehingga gate tidak dinilai ulang |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — tidak ada batas konteks, kepemilikan data, billing, atau aturan keselamatan klinis yang bergeser. HD/LD tetap dibaca dari ringkasan server (`RWI-DEC-302`, `RWI-DEC-303`) |
| Bukti as-is | Impact scan terfokus read-only bagian 21.2 pada frontend `a491b05afcb6b3ed70218ccaa557007719d45ac3` dan backend `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` |
| Rencana kerja | [planning-redesain-tampilan-denah-bed.md](./docs/map-bed/planning/planning-redesain-tampilan-denah-bed.md) |
| Dampak backend dan API | **Nol.** Tidak ada endpoint, DTO, tabel, kolom, migration, atau permission baru (`RWI-DEC-343`). `02-backend-architecture.md`, `data/`, `flowcharts/`, dan kelima berkas `contracts/` tidak berubah |

### 21.1 Bagian lama yang digantikan

Bagian 16 tetap berlaku kecuali butir di bawah. Butir lama tidak dihapus supaya riwayat persetujuan
16 Oktober 2026 tetap terbaca.

| Bagian lama | Isi lama | Sekarang |
| --- | --- | --- |
| 16.1 baris "Brief produk" | "Desain denah tidak diubah" kecuali permintaan pemilik | Rupa denah diganti peta bangsal (`RWI-DEC-343`, `RWI-DEC-344`). Kamar penuh tanpa merah, tepi hijau untuk kamar yang bisa dipilih, pop up, dan alur memilih tetap berlaku |
| 16.1 baris `DEV_DISCRETION` | Didelegasikan "sebatas tidak mengubah tampilan peta" | Diperluas oleh 21.3: rupa peta boleh dirancang ulang dalam batas 21.5–21.7 |
| 16.4 baris `floor-plan-drawing` | Tampilan dibuktikan sama lewat perbandingan screenshot per lantai | Gambar CAD tidak dirender lagi setelah keenam lantai selesai (`RWI-DEC-345`); bukti tampilan memakai 21.9 |
| 16.6 kalimat bukti tampilan | Screenshot sebelum dan sesudah "berbeda hanya pada data" | Diganti pemeriksaan 21.9. Syarat "layar 390 px tidak bergeser horizontal" tetap berlaku |
| 16.7 butir 3 | "Tampilan floor plan ... tidak boleh diubah" | Digantikan `RWI-DEC-343`; butir 1, 2, dan 4 tetap berlaku |

Rupa status yang masuk ke source lewat commit frontend `685844a34` (17 Oktober 2026) — tepi teal,
kamar penuh berwarna oranye atau hijau, dan aturan suara terbanyak untuk HD/LD — tidak pernah tercatat
di blueprint. Rupa itu dicabut `RWI-DEC-346` (`RWI-CON-018`).

### 21.2 Bukti as-is — impact scan terfokus

Semua bukti dibaca read-only dari `HEAD`. Status memakai istilah capability map.

| No | Fakta | Bukti | Status | Dampak pada desain |
| ---: | --- | --- | --- | --- |
| 1 | Gambar CAD — dinding, pintu, tangga, garis sumbu, angka ukuran, label ruang teknis, judul "3rd Floor" — digambar di bawah kamar | FE `floor-plan-drawing.jsx:47-76`; dikirim lewat prop `drawing` (`floor-plan-view.jsx:136`, `:345`) oleh `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, dan `master-data-bed-floor-plan-view.jsx` | `Replace` | Lapisan peta baru dipasang di titik yang sama (21.8) |
| 2 | Bentuk kamar dan kunci database | FE `lantai-N.plan.js` (`rooms[]`); `floor-plan-room-map.js:49` (`lantai-<n>/<slug>`) | `Reuse` | Tidak diubah; kartu kamar memakai bentuk ini |
| 3 | Label HD/LD kamar penuh dihitung frontend dengan suara terbanyak | FE `floor-plan-room.jsx:94-95` | `Repair` | Diganti `dependencySummary.isHighDependency` dari server (21.6) |
| 4 | Warna status kamar | FE `floor-plan.module.css:48` (tersedia = warna utama/teal), `:63` (penuh HD oranye), `:68` (penuh LD hijau), `:73` (ungu), `:91` (kelompok kelas khusus hijau) | `Repair` | Diganti aturan 21.6 dan `RWI-FE-009` |
| 5 | Data denah sudah memuat semua yang dibutuhkan | FE `use-floor-plan.js:155` (ringkasan HD/LD), `:165` (penuh), `:174` (pesanan aktif), `:219` (bed kosong per lantai) | `Reuse` | Ringkasan lantai dan angka tab dihitung tanpa API baru |
| 6 | Server menghitung bed kosong per lantai dan label HD | BE `InpFloorPlanQueryService.cs:139` (`AvailableBedCount`), `:260` (`IsHighDependency = HighCount >= 1`); DTO `InpatientFloorPlanDtos.cs:33-47` | `Reuse` | Tanpa perubahan backend |
| 7 | Profil Housekeeping tidak menerima HD/LD maupun konteks penghuni | BE `InpFloorPlanQueryService.cs:199`; `GUARD-DNH-01` | `Reuse` | Kartu profil ini tanpa label HD/LD dan tanpa ikon pesanan (21.6) |
| 8 | Warna titik status bed di pop up | FE `floor-plan-room-popup.jsx:57` (`statusTone`); `floor-plan.module.css:1908-1934`: tersedia = warna utama/teal, terisi LD = hijau, menunggu pembersihan = oranye | `Conflict` → `Repair` | Bertabrakan dengan `RWI-DEC-346` (hijau = tersedia) dan `RWI-DEC-347` (titik kartu = warna pop up); diselesaikan `RWI-DEC-353` dengan satu palet untuk kartu dan pop up (21.12) |
| 9 | Mode Master tidak punya status "bisa dipilih" dan tidak menerima daftar lantai dari server | FE `use-floor-plan-layout.js:287`, `:314` (`isSelectable: false`), `:293`, `:309` (label ruang cadangan dan belum dipetakan); BE `FloorPlanLayoutDtos.cs:19-26` (tanpa `Floors`) | `Reuse` | Master tanpa tepi hijau; tab Master tanpa angka bed kosong (21.5) |
| 10 | Kamar dengan `isSelectable = false` tidak dapat difokus keyboard di **semua** mode, sehingga kamar penuh di Monitoring dan seluruh kamar di Master hanya dapat dibuka dengan tetikus | FE `floor-plan-room.jsx:79`, `:132` (`tabIndex = -1`); gaya redup hanya untuk mode Pilih (`floor-plan.module.css:1343`) | `Repair` | Bertentangan dengan 16.6 dan `RWI-DEC-303` (kamar penuh tetap dapat dibuka). Aturan fokus baru di 21.9 |

Kesegaran: working tree frontend bersih saat pemeriksaan. Perubahan dokumen paralel di backend
(`roadmap/*-prioritas-antrean-member.md` dan laporan `BE-RWI-252`/`253`) bukan bagian amandemen ini.

### 21.3 Hierarki wewenang

| Tingkat | Yang dikunci | Sumber |
| --- | --- | --- |
| Keamanan, privasi, invariant | Peta, kartu, kartu ringkas, dan ringkasan lantai tidak memuat nama pasien, No. RM, jenis kelamin, lencana tier, maupun Guid. HD/LD hanya tampil bila server mengirimkannya. 106 kamar, kunci denah, dan posisi kamar tetap | `RWI-DEC-287`, `315`, `341`, `343`; `GUARD-DNH-01` |
| Brief produk yang disetujui | Peta bangsal (21.5), satu peta di empat pemakai tanpa CAD, rupa status (21.6), titik bed (21.7), ringkasan lantai dan angka tab, contoh Lantai 3 lebih dulu (21.10), satu palet titik status (21.7) | `RWI-DEC-344` s.d. `349`, `RWI-DEC-353`; asumsi `RWI-DEC-350`, `351` disahkan `RWI-DEC-352` |
| Konvensi project | Base component dan references `build-module-frontend` tidak wajib; komponen tinggal di folder feature denah dengan CSS module sendiri; struktur route → view → hook → service; lint dan build | `RWI-DEC-325` butir 1, 2, 4 |
| `DEV_DISCRETION` | Warna kelompok kelas (`RWI-FE-009`); bentuk lantai, koridor, penanda, dan blok zona (`RWI-FE-010`); kartu ringkas, animasi, ikon, dan huruf (`RWI-FE-011`); nasib berkas CAD (`RWI-FE-012`); ukuran, jarak, sudut, dan bayangan | Decision log revision 62 |

### 21.4 Layar dan peta butir menu

Tidak ada butir menu, route, maupun layar baru. Peta lintas sub-modul tetap di
[02-module-map.md bagian 10](../02-module-map.md) dan bagian 16.2.

| ID | Layar | Perubahan |
| --- | --- | --- |
| FE-DNH-01 | Komponen denah bersama | **Diperbarui**: lapisan peta, kartu kamar, ringkasan lantai, legenda |
| FE-BM-01 | Monitoring Bed | Memakai FE-DNH-01 baru; ringkasan dan angka tab tampil |
| — | Langkah Pilih Bed admisi (termasuk admisi bayi), transfer pada detail episode, panel transfer Workspace Keperawatan | Memakai FE-DNH-01 baru dalam mode Pilih |
| FE-DNH-03 | Master Bed — editor denah | Memakai FE-DNH-01 baru dalam mode Master |
| FE-DNH-02 | Pop up kamar | **Tidak berubah**, kecuali warna titik status (`RWI-DEC-353`) |

### 21.5 Skema fitur — peta bangsal

Monitoring Bed, Lantai 3. Angka hanya contoh.

```text
Bed Management › Monitoring Bed (FE-BM-01)
Tab       : Lantai 2 (0) | [Lantai 3 (20)] | Lantai 4 (8) | Lantai 5 (3) | Lantai 8 (5) | Lantai 9 (9)
Ringkasan : 48 bed · 20 tersedia · 22 terisi · 2 dipesan · 3 persiapan · 1 tidak tersedia
Kelas     : (VIP 17) (VIP DELUXE 1) (ODC 5) (KELAS II 5) (KELAS I 1)
Legenda   : [hijau] bed tersedia · [ungu] pesanan menunggu masuk · [abu] penuh · HD/LD · 3/2
Peta      : lempeng lantai
            ├─ penanda   : lift, tangga, NS3A, NS3B, pantry, toilet, spoel hoek
            ├─ baris atas: 330 329 328 327 326 325 · inti layanan · 322 321A 321B 320 319
            ├─ koridor
            └─ baris bawah: 301 302 303 … 316 317 318
Alat      : [−] 100% [+] · layar penuh · kompas
```

Anatomi kartu kamar:

```text
 biasa (lebar ≥ 56 unit)          ringkas (lebar atau tinggi < 56 unit)
╭───────────────╮                 ╭────────────╮
│▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀│ ← strip kelas    │▀▀▀▀▀▀▀▀▀▀▀▀│
│   KELAS II    │ ← nama kelas     │ BAYI-1 1/0 │ ← nomor + badge satu baris
│               │                  │   ▪        │ ← titik bed
│      329      │ ← nomor kamar    ╰────────────╯
│               │
│    ▪ ▪ ▫      │ ← satu titik per bed
│     3/2       │ ← badge kapasitas/terisi
╰───────────────╯
```

| Wilayah | Isi | Data dan sumber | Hak akses | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Tab lantai | Nama lantai dan jumlah bed kosong | `Floors[].FloorName` dan `Floors[].AvailableBedCount` dari `GET bed-management/floor-plan` (Monitoring) atau `GET bed-occupancies/floor-plan` (Pilih). Master: daftar lantai bawaan frontend, tanpa angka | Mengikuti layar pemakai | Respons tanpa `Floors`: tab dari daftar lantai bawaan, angka disembunyikan |
| Ringkasan lantai | Total bed dan jumlah per status | Dihitung dari `Rooms[].Beds[].StatusCode` lantai aktif. Persiapan = `WaitingCleaning` + `Cleaning`. Master memakai bed dari `GET rooms/floor-plan-layout` | — | Lantai tanpa kamar berkunci: "Belum ada kamar pada lantai ini" (16.3). Angka tidak pernah diambil dari lantai lain |
| Legenda kelas | Kelas dan jumlah kamar | `Rooms[].PatientClassName` | — | Tidak tampil bila kosong |
| Legenda status | Bed tersedia, Pesanan menunggu masuk, Penuh, label HD/LD, Kapasitas/terisi. Pilih: ditambah Bed terpilih. Master: ditambah Ruang belum aktif | Tetap (statis) | — | — |
| Peta | Lempeng lantai, koridor, blok zona unit lain, penanda | Data skematik konstanta frontend (21.8), bukan API | — | Lantai tanpa data skematik selama masa contoh: gambar lama (`RWI-DEC-351`) |
| Kartu kamar | Kelas, nomor, badge, titik bed, rupa status | `FloorPlanRoomKey`, `PatientClassName`, `RoomNumber`, `Counts`, `RoomStatus`, `IsSelectable`, `DependencySummary`, `Beds[].StatusCode`, `Beds[].HoldingContext` | Klik membuka FE-DNH-02; aksi di dalamnya tetap mengikuti 16.3 | Bentuk tanpa data API: garis putus-putus tanpa nomor, tidak dapat diklik maupun difokus |
| Kartu ringkas | Kelas, nomor, jumlah bed per status, status kamar | Data kartu yang sama | — | Tidak memuat identitas pasien |
| Alat | Zoom, geser, Ctrl + Scroll, layar penuh, kompas | — | — | — |

Base URL endpoint: `api/v1/health-services/inpatient-management/bed-management`,
`api/v1/health-services/inpatient-management/bed-occupancies`, dan
`api/v1/health-services/master-data/rooms`. Ketiganya sudah ada dan tidak berubah (API contract 14.2).

### 21.6 Rupa status kamar (`RWI-DEC-346`, `RWI-DEC-350`)

Aturan dibaca dari atas; aturan pertama yang cocok menentukan rupa kartu.

| Urutan | Keadaan | Rupa | Contoh |
| ---: | --- | --- | --- |
| 1 | Mode Pilih: kamar tidak punya bed yang layak untuk pasien ini (`IsSelectable = false`) | Kartu diredupkan dan tidak dapat difokus | Pasien isolasi: semua kamar tanpa bed isolasi yang tersedia redup |
| 2 | Mode Pilih: kamar berisi bed yang sedang dipilih | Isi amber, halo amber, ikon centang | 329-C dipilih → kartu 329 amber |
| 3 | Masih ada bed tersedia (`IsSelectable = true`) | Tepi hijau 2 px. Bila ada pesanan menunggu masuk, ditambah ikon jam ungu | 328 (Terisi, Dipesan, Tersedia) → tepi hijau, ikon ungu, `3/2` |
| 4 | Tidak ada bed tersedia dan ada pesanan menunggu masuk | Isi ungu tipis, tepi ungu, ikon jam | 328 setelah bed terakhirnya dipesan → ungu, `3/3` |
| 5 | Penuh tanpa pesanan | Isi abu netral tanpa tepi berwarna; tetap dapat diklik | 329 berisi 3 pasien → abu, `3/3` |
| 6 | Tidak ada bed yang dapat dipakai karena persiapan atau tidak tersedia | Isi abu muda, tepi abu putus-putus, teks redup | Kamar 505 yang semua bednya menunggu pengesahan |
| 7 | Bentuk ada di denah tetapi kamar tidak ada di respons API | Garis putus-putus tanpa nomor; tidak dapat diklik | Kunci kamar belum diimpor |

Label HD/LD hanya dipasang pada kamar penuh (urutan 4 dan 5) dan dibaca dari `DependencySummary`:

| Ringkasan dari server | Label | Contoh |
| --- | --- | --- |
| `IsHighDependency = true` (sekurang-kurangnya satu penghuni HD) | "HD" oranye | 329: 1 HD + 2 LD → "HD" |
| `IsHighDependency = false` dan `LowCount ≥ 1` | "LD" abu | 3 penghuni LD → "LD" |
| `HighCount = 0` dan `LowCount = 0` (semua belum dinilai) | Tanpa label (`RWI-DEC-350`) | 330 berisi 2 pasien yang belum dinilai |
| `DependencySummary` tidak dikirim (profil Housekeeping, `GUARD-DNH-01`) | Tanpa label | — |

Kamar yang belum penuh tidak diberi label HD/LD; penghuni HD di kamar itu terbaca dari titik bed
(21.7) dan pop up.

**Mode Master.** Semua kamar memakai urutan 5–7 tanpa tepi hijau, karena data susunan tidak memuat
kelayakan. Ruang cadangan dan kamar belum dipetakan tampil putus-putus dengan label "Ruang cadangan"
atau "<kode> belum dipetakan".

**Profil Housekeeping.** Server tidak mengirim `HoldingContext` dan `DependencySummary`, sehingga
kartu tanpa label HD/LD dan tanpa ikon pesanan. Titik bed tetap menunjukkan status, misalnya Dipesan.

**Pembaca layar.** Status tidak hanya dibawa warna. Label kamar (`aria-label`) menyebut kelas, nomor,
jumlah terisi, penuh atau tidak, "ada pasien High Dependency" bila `IsHighDependency = true`, dan "ada
pesanan menunggu konfirmasi masuk" bila ada pesanan. Contoh: "KELAS II 329, 3 dari 3 tempat tidur
terisi, penuh, ada pasien High Dependency".

### 21.7 Titik bed (`RWI-DEC-347`)

1. Satu titik per bed, berurutan menurut label bed (A, B, C, dan seterusnya). Kamar dengan lebih dari
   enam bed tidak memakai titik; badge saja.
2. Warna titik sama dengan nada status baris bed di pop up (`statusTone`): tersedia, terisi HD, terisi
   LD, terisi belum dinilai, dipesan, menunggu pembersihan, dalam pembersihan, dan tidak tersedia.
   Kartu dan pop up memakai satu palet (`RWI-DEC-353`): hijau hanya tersedia, ungu hanya dipesan, oranye
   hanya terisi HD.
3. Titik dan kartu ringkas tidak memuat identitas; kartu ringkas menyebut jumlah per status.

**Contoh:** KELAS II 329 dengan 329-A terisi pasien LD, 329-B dipesan, dan 329-C tersedia tampil
dengan tiga titik — terisi LD, dipesan, tersedia — dan badge `3/2`.

**Penyelarasan palet (`RWI-DEC-353`).** Palet pop up lama memakai teal untuk tersedia, hijau untuk
terisi LD, dan oranye untuk menunggu pembersihan. Palet itu diganti satu palet bersama untuk kartu dan
pop up. Warna status selain ketiga warna kunci adalah `DEV_DISCRETION` dengan syarat bukan hijau, ungu,
atau oranye. Requirement-nya `FR-DNH-025` (`EPIC DNH-08`), dikirim bersama contoh Lantai 3.

### 21.8 Struktur target

```text
QuilvianSystemFrontendDev/src/
├─ components/features/health-services/inpatient-management/floor-plan/
│  ├─ floor-plan-schematic.jsx        [Baru; lempeng lantai, koridor, zona, penanda — tanpa state]
│  ├─ floor-plan-summary.jsx          [Baru; ringkasan bed lantai aktif]
│  ├─ floor-plan-room-hovercard.jsx   [Baru; kartu ringkas saat kursor atau fokus]
│  ├─ floor-plan-room.jsx             [Diperbarui; kartu kamar, aturan 21.6, titik bed 21.7]
│  ├─ floor-plan-view.jsx             [Diperbarui; pilih lapisan, ringkasan, angka tab, viewBox skematik, tanpa judul lembar]
│  ├─ floor-plan-monitoring.jsx       [Diperbarui; satu baris lapisan peta]
│  ├─ floor-plan-selection.jsx        [Diperbarui; satu baris lapisan peta]
│  ├─ floor-plan-skeleton.jsx         [Diperbarui; kerangka berbentuk kartu]
│  ├─ floor-plan-compass.jsx          [Diperbarui; gaya saja]
│  ├─ floor-plan.module.css           [Diperbarui; token status 21.6, kartu, lapisan peta; gaya CAD dihapus setelah tidak dipakai]
│  ├─ floor-plan-drawing.jsx          [Sudah ada; dipakai sebagai cadangan selama masa contoh, berkas disimpan — RWI-FE-012]
│  ├─ floor-plan-room-popup.jsx       [Sudah ada; hanya warna titik status yang berubah — RWI-DEC-353]
│  ├─ floor-plan-master-room-popup.jsx [Sudah ada; tidak berubah]
│  └─ zoom-toolbar.jsx                [Sudah ada; tidak berubah]
├─ components/view/health-services/master-data/bed/master-data-bed-floor-plan-view.jsx [Diperbarui; satu baris lapisan peta]
├─ lib/constants/health-services/inpatient-management/floor-plan/
│  ├─ schematic/schematic-geometry.js [Baru; insetOrthogonal, roundedOrthogonalPath, largestRect]
│  ├─ schematic/lantai-3.schematic.js [Baru; contoh pertama]
│  ├─ schematic/lantai-{2,4,5,8,9}.schematic.js [Baru; sesudah pengesahan contoh]
│  ├─ floors.js                       [Diperbarui; properti schematic per lantai]
│  ├─ lantai-N.plan.js                [Sudah ada; rooms[] dipakai ulang, tidak diubah]
│  └─ floor-plan-room-map.js          [Sudah ada; tidak diubah]
├─ lib/hooks/health-services/inpatient-management/use-floor-plan.js     [Sudah ada; tidak diubah]
├─ lib/hooks/health-services/master-data/bed/use-floor-plan-layout.js   [Sudah ada; tidak diubah]
└─ lib/services/**                                                       [Sudah ada; tidak diubah]
```

Data skematik satu lantai berbentuk konstanta beku. Satuannya sama dengan gambar sumber (1536 × 1024)
sehingga dapat dicocokkan langsung dengan `rooms[]`:

```js
export const LANTAI_3_SCHEMATIC = Object.freeze({
  viewBox: { x: 56, y: 230, width: 1424, height: 700 }, // dipangkas ke area gedung
  footprint: [[82, 300], [1436, 300], [1436, 907], [82, 907]],
  corridors: [{ x: 148, y: 697.5, width: 1280, height: 66.5, label: "Koridor" }],
  zones: [{ name: "Ka. dep Peralatan", points: [/* ... */] }],
  landmarks: [{ type: "nurse-station", label: "NS3A", x: 655, y: 640 }],
});
```

Angka di atas adalah perkiraan awal dari geometri kamar dan dicocokkan dengan gambar sumber saat
pengerjaan. Kartu kamar dibentuk dari poligon `rooms[]` yang dimasukkan 3 unit ke dalam
(`insetOrthogonal`) dan diberi sudut bulat (`roundedOrthogonalPath`); teks ditempatkan di kotak
terbesar di dalam poligon (`largestRect`). Ketiga fungsi itu JavaScript murni sehingga dapat diperiksa
dengan skrip Node.

**Pemilihan lapisan.** `floors.js` memberi properti `schematic` pada lantai yang datanya sudah ada.
`FloorPlanView` menggambar `FloorPlanSchematic` bila `schematic` ada, dan gambar lama bila belum
(masa contoh, `RWI-DEC-351`). Setelah keenam lantai selesai, gambar lama tidak dipanggil lagi.

### 21.9 Keadaan, aksesibilitas, dan verifikasi

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka berbentuk kartu, bukan kerangka CAD |
| Gagal | Tetap seperti 16.3: "Denah gagal dimuat. Muat ulang." dengan tombol Coba Lagi |
| Data basi | Tetap seperti 16.5 dan amandemen realtime `0.16.0`: satu lantai satu panggilan, respons lama dibatalkan, waktu data server tampil di petunjuk |
| Kirim ganda | Tidak ada pengiriman dari peta; pop up tetap memakai kunci idempotensi (16.5) |
| Lantai tanpa data skematik | Gambar lama selama masa contoh (`RWI-DEC-351`) |

**Aksesibilitas.** Kartu dapat difokus dan dibuka dengan Enter atau Spasi; fokus kembali ke kartu saat
pop up ditutup. Pada Monitoring dan Master, **semua** kartu yang punya data dapat difokus, termasuk
kamar penuh; hanya mode Pilih yang membuat kartu tidak layak tidak dapat difokus dan bertanda
`aria-disabled` (21.2 no. 10). Kontras teks nomor kamar sekurang-kurangnya 4,5 : 1. Animasi masuk dimatikan bila
pengguna memilih gerak dikurangi (`prefers-reduced-motion`). Halaman selebar 390 px tidak bergeser
horizontal (16.6).

**Bukti tampilan, pengganti kalimat bukti di 16.6:**

1. Screenshot keenam lantai di Monitoring Bed, Pilih Bed, dan Master Bed, dibandingkan dengan gambar
   acuan `docs/map-bed/UI-Sudah-Diterapkan/`, membuktikan semua kamar ada di posisi yang sama.
2. Skrip Node geometri (`DNH-AT-918`).
3. Jejak jaringan: satu panggilan API per lantai (`DNH-AT-922`).
4. `lint:errors` bersih; `build` dijalankan pemilik dan dicatat `NOT RUN` bila belum.

### 21.10 Urutan pengerjaan (`RWI-DEC-349`, `RWI-DEC-351`)

1. Contoh Lantai 3: fondasi geometri, data skematik Lantai 3, kartu kamar, lapisan peta, dan kepala
   halaman di keempat pemakai.
2. Atasan melihat Lantai 3. Revisi yang diminta dikerjakan di Lantai 3 lebih dulu. Muhammad Hamzah
   mencatat pengesahannya sebagai keputusan baru di decision log.
3. Lantai 2, 4, 5, 8, dan 9.
4. Rilis ke pengguna rumah sakit setelah keenam lantai selesai.

### 21.11 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Filter kelas, pilihan "hanya kamar tersedia", pencarian nomor kamar | Ditunda `RWI-DEC-348`; dibahas lagi setelah pengesahan atasan |
| Tombol "tampilkan denah asli (CAD)" | `RWI-DEC-345` |
| Tampilan daftar dan versi ponsel | Di luar batas scope decision log revision 61 |
| Endpoint atau field ringkasan baru, termasuk angka bed kosong untuk Master | `RWI-DEC-343`: tanpa perubahan backend; ringkasan dihitung dari data yang sudah ada |
| Base component bersama untuk kartu kamar | `RWI-DEC-325` butir 4 dan `AGENTS.md` frontend |
| Menghapus berkas gambar CAD | `RWI-FE-012`; `rooms[]` di berkas yang sama tetap dipakai registri kamar dan importer |

### 21.12 `PETA-OPEN-01` — warna titik status — diputuskan A (`RWI-DEC-353`)

`RWI-DEC-346` memakai hijau untuk "bed tersedia". `RWI-DEC-347` meminta titik bed berwarna sama dengan
pop up. Pop up saat ini memakai teal untuk tersedia dan hijau untuk terisi Low Dependency (21.2 no. 8),
sedangkan `RWI-DEC-343` menyatakan pop up tidak berubah. Ketiganya tidak dapat dipenuhi bersamaan.

A. Warna titik status di pop up diselaraskan dengan peta — hanya warna titik; isi, tata letak, dan
   aksi pop up tetap **(Direkomendasikan)**. Satu bahasa warna di kartu dan pop up: hijau hanya berarti
   tersedia, ungu hanya pesanan, oranye hanya HD. Konsekuensi: "pop up tidak berubah" pada
   `RWI-DEC-343` diperjelas tidak mencakup warna titik.
B. Pop up tetap persis; titik di kartu memakai palet baru yang selaras dengan peta. Konsekuensi: titik
   kartu dan titik pop up berbeda warna untuk status yang sama; `RWI-DEC-347` diamandemen.
C. Pop up tetap dan titik kartu memakai palet pop up apa adanya. Konsekuensi: titik hijau berarti
   terisi LD, padahal tepi hijau berarti tersedia.
D. Other — tuliskan pilihan atau batasan lain.

**Jawaban pemilik, 2026-10-20:** opsi A, lewat "iya saya setujui semua" (`RWI-OQ-165` → `RWI-DEC-353`).
`EPIC DNH-08` tidak lagi tertahan dan dikirim bersama contoh Lantai 3.
