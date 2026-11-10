# Rehab Medik — PRD → MVP

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk / Modul | Quilvian V2 / Rehab Medik (`RehabilitationManagement`, prefix `Rhb`) |
| Kode modul | `RHB` |
| Status | `draft` — menunggu approval owner (Sukma Giri Pratama) |
| Repository | BE `NewQuilvianSystemBackend@sukmagp` `6bb3fba7`; FE `QuilvianSystemFrontendDev@sukmagpV2` `0c06c093` |
| Cakupan | Satu episode rehab dari permintaan sampai episode ditutup, termasuk penagihan sesi terapi. Tanpa jadwal, perangkat, dan SATUSEHAT. |

## 2. Ringkasan eksekutif

Unit Rehab Medik membutuhkan satu catatan episode yang dapat diaudit. Catatan itu memuat siapa
yang meminta, siapa yang menerima, apa hasil asesmen, program apa yang disepakati, terapi apa
yang benar-benar diberikan, bagaimana responsnya, dan kapan episode berakhir. Selain itu, setiap
terapi yang diberikan harus tertagih tepat satu kali. MVP mengirim jalur ini secara utuh untuk
fisioterapi, terapi okupasi, dan terapi wicara di bawah tanggung jawab dokter Sp.KFR.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
|---|---|
| Belum ada modul rehab di BE maupun FE | `01-existing-capability-map.md` C-19, F-02 |
| Tab Rehab di workspace dokter rawat inap masih "belum tersedia" | F-09 |
| Fisioterapis rawat inap hanya bisa menulis CPPT naratif | C-20 |
| Billing menolak charge manual untuk RJ/RWI, sehingga terapi tanpa sumber klinis tidak tertagih | `BillingInvoiceService.cs:2933` `RJE-VAL-010` |
| Sudah tersedia: keutuhan dokumen + addendum RM, mesin instrumen berversi, lookup encounter, penomoran, adapter fakta charge | C-11..C-16 |

## 4. Visi produk

Permintaan → episode → asesmen medis → resep layanan → asesmen profesi → tujuan → program →
edukasi/persetujuan → sesi pada kunjungan nyata → tindakan bertarif → reasesmen → ringkasan
akhir → episode ditutup. Setiap dokumen klinis terkunci saat ditandatangani dan terbaca di berkas
rekam medis pasien.

## 5. Batas MVP

**Titik mulai**
1. Admisi rehab mendaftarkan permintaan untuk pasien yang sudah punya kunjungan RJ/RWI/IGD,
   atau membawa rujukan luar.

**Titik akhir**
1. Episode `Closed` lewat ringkasan akhir yang ditandatangani.
2. Seluruh sesi terlaksana sudah menjadi item tagihan di Billing tanpa ganda.
3. Seluruh dokumen klinis episode terbaca di berkas rekam medis.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
|---|---|
| Petugas admisi rehab | Mendaftarkan, melengkapi, mengembalikan, membatalkan permintaan |
| Dokter Sp.KFR | Disposisi, asesmen medis + resep layanan, reasesmen medis, ringkasan akhir |
| Fisioterapis / terapis okupasi / terapis wicara | Asesmen profesi, tujuan, program, edukasi, sesi, reasesmen disiplin |
| Petugas billing | Memantau dan mengulang handoff yang gagal |
| Rekam medis / auditor | Membaca, addendum lewat RM |
| Admin konfigurasi + komite | Layanan rehab, tindakan, instrumen (disahkan komite) |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Permintaan, validasi, duplikasi, disposisi | FEAT-001..003; C-02, C-03, C-05 | Wajib. Tanpa ini episode tidak punya asal yang sah. |
| Asesmen medis Sp.KFR + resep layanan | FEAT-007; RHB-DEC-005 | Wajib. Prasyarat setiap program. |
| Asesmen profesi fisio/okupasi/wicara | FEAT-008..010; C-09 | Wajib. Prasyarat program disiplin. |
| Instrumen (subset) + pengukuran terstruktur | RHB-DEC-017; C-15 | Wajib. Tanpa baseline, reasesmen tidak bermakna. |
| Tujuan dan program berversi | FEAT-012, 013 | Wajib. Sesi hanya merujuk program aktif. |
| Edukasi dan persetujuan | FEAT-015 | Wajib. Penolakan pasien harus menghentikan aktivasi. |
| Sesi: keselamatan, tindakan aktual, deviasi | FEAT-016..018 | Wajib. Inti layanan. |
| Handoff billing sesi | FEAT-032; RHB-DEC-021; C-21 | Wajib. Tanpa ini terapi tidak tertagih (`RJE-VAL-010`). |
| Reasesmen dan keputusan | FEAT-024, 025 | Wajib. Satu-satunya jalan modifikasi/hentikan program. |
| Ringkasan akhir + tindak lanjut | FEAT-026, 027 | Wajib. Satu-satunya jalan menutup episode. |
| Finalisasi + addendum | FEAT-028, 029; C-11, C-12 | Wajib. Kewajiban RME. |

## 8. Kemampuan yang ditunda

| Kemampuan | ID | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Antrean lintas setting, jadwal seri, kehadiran | FEAT-004..006 | Aturan slot, prioritas, dan daftar tunggu belum diputuskan (UNR-005) | Worklist permintaan/episode/sesi + filter "evaluasi jatuh tempo". Kunjungan RJ didaftarkan lewat modul Registrasi. Tidak hadir dicatat sebagai sesi `NotDone`. |
| Koordinasi multidisiplin terpadu | FEAT-014 | Butuh model konferensi tim yang belum dirancang | Ruang kerja episode menampilkan semua layanan, program, dan sesi berdampingan |
| Ortotik-prostetik (asesmen, work order, fitting, serah terima) | FEAT-011, 019..021 | Layanan belum dibuka di rilis 1 (RHB-DEC-004). Butuh integrasi pengadaan. | Disiplin `ORTHOTIC_PROSTHETIC` ada tetapi tidak aktif. Kebutuhan alat dicatat di rencana/tindak lanjut. |
| Program rumah + teach-back terstruktur | FEAT-022, 023 | Tergantung media edukasi yang disahkan | Rekaman edukasi (FE-RHB-09) dan tindak lanjut `HomeProgram` pada ringkasan akhir |
| SATUSEHAT + antrean kirim ulang | FEAT-030, 031 | Profil resource rehab belum dipastikan (UNR-008) | Dokumen tersimpan lokal dan terkunci |
| Rekonsiliasi bukti klaim | FEAT-033 | Aturan JKN belum dikonfirmasi (UNR-006/007) | Tagihan berbasis tindakan final dengan kunci idempoten |
| COPM, MMT/ROM per sendi, TADIR di mesin instrumen | RHB-OQ-013 | Mesin skor belum mendukung | Pengukuran terstruktur |
| Tautan otomatis CPPT ↔ sesi | RHB-OQ-014 | Butuh kesepakatan pemilik CPPT | Terapis menulis ringkasan rujukan di CPPT secara manual |

## 9. Alur bisnis target

`FLOW-RHB-MVP-001` — lihat `flowcharts/00-alur-utama.md`:
1. Admisi mendaftarkan dan mengajukan permintaan.
2. Sp.KFR menerima, dan episode terbuka.
3. Sp.KFR menandatangani asesmen medis dan meresepkan layanan.
4. Terapis menandatangani asesmen profesi.
5. Terapis menyusun tujuan dan program, mengedukasi pasien, lalu mengaktifkan program.
6. Setiap kunjungan: terapis memeriksa keselamatan, memberi terapi, dan menandatangani sesi.
   Tindakan diteruskan ke Billing.
7. Pada tanggal evaluasi, terapis/Sp.KFR melakukan reasesmen dan memutuskan kelanjutan.
8. Sp.KFR menandatangani ringkasan akhir, dan episode ditutup.

## 10. Epic dan functional requirement

### EPIC RHB-00 — Fondasi dan konfigurasi
Disposisi backend: `MISSING / NEW` (modul) + `EXTEND` (enum RM/Cli, `PatientProcedureSource`).
- **FR-RHB-004** — Lookup kunjungan hanya mengembalikan kunjungan terbuka milik pasien yang
  diminta. *Contoh:* lookup pasien A tidak pernah memuat kunjungan pasien B.
- **FR-RHB-062** — Butir menu hanya tampil sesuai hak akses. URL langsung tanpa hak akses
  menampilkan layar akses ditolak.
- **FR-RHB-070** — Layanan rehab tidak bisa diaktifkan tanpa profesi klinis (`RHB-VAL-070`).

### EPIC RHB-01 — Permintaan dan disposisi (FEAT-001..003)
Disposisi: `MISSING / NEW`.
- **FR-RHB-001** — Permintaan tanpa pasien, asal, indikasi, layanan, atau waktu ditolak saat
  diajukan. Nomor unik dibuat server.
- **FR-RHB-002** — Duplikasi memunculkan peringatan 409 beserta kandidat. Melanjutkan wajib
  dengan alasan. Tidak ada penghapusan otomatis.
- **FR-RHB-003** — Hanya pemegang `RehabRequest:Disposition` yang dapat menerima/mengalihkan/
  menolak. Setiap disposisi mencatat pelaku, waktu, keputusan, dan alasan. Penerimaan membuka
  episode dalam transaksi yang sama.

### EPIC RHB-02 — Asesmen (FEAT-007..010)
Disposisi: `MISSING / NEW` + `EXISTING / REUSE` (keutuhan RM, mesin instrumen).
- **FR-RHB-011** — Tanda tangan asesmen medis wajib meresepkan ≥1 layanan. Episode lalu
  berpindah ke `Planning`.
- **FR-RHB-012** — Asesmen profesi hanya oleh profesi yang sesuai, untuk layanan yang sudah
  diresepkan.
- **FR-RHB-013** — Asesmen final tidak dapat diubah (`RHB-VAL-021`).
- **FR-RHB-014** — Koreksi lewat addendum RM, dan catatan asli tetap terbaca.
- **FR-RHB-015** — Pengukuran berinstrumen dan terstruktur tersimpan dengan instrumen dan
  satuannya.
- **FR-RHB-016** — Instrumen yang belum disahkan tidak dapat dipakai di luar lingkungan yang
  mengizinkan konsep.

### EPIC RHB-03 — Tujuan, program, edukasi (FEAT-012, 013, 015)
Disposisi: `MISSING / NEW`.
- **FR-RHB-010** — Program tidak dapat aktif sebelum asesmen medis dan asesmen disiplin final.
- **FR-RHB-020** — Edukasi berstatus menolak memblokir aktivasi program terkait.
- **FR-RHB-021** — Revisi menghasilkan versi baru. Versi lama `Superseded`, dan hanya satu versi
  aktif per layanan.
- **FR-RHB-022** — Dua penyimpanan bersamaan: satu berhasil, satu 409.

### EPIC RHB-04 — Sesi terapi (FEAT-016..018)
Disposisi: `MISSING / NEW` + `EXISTING / REUSE` (encounter, InpEpisode).
- **FR-RHB-030** — Sesi hanya dimulai pada kunjungan terbuka milik pasien.
- **FR-RHB-031** — Pasien rawat inap yang sudah pulang secara fisik tidak dapat diberi sesi.
- **FR-RHB-032** — Sesi tidak dapat dimulai tanpa verifikasi identitas dan program.
- **FR-RHB-036** — Henti dini wajib alasan dan tindakan segera.
- **FR-RHB-037** — Deviasi yang memengaruhi sesi berikutnya menandai program "perlu telaah".

### EPIC RHB-05 — Handoff billing (FEAT-032)
Disposisi: `EXTEND` (`TrxPatientProcedure`, `ClinicalMilestoneFactProducer`, pola HD).
- **FR-RHB-033** — Tanda tangan sesi terlaksana membentuk satu tindakan bertarif per intervensi
  dan meneruskannya ke Billing. *Contoh:* sesi dengan TENS dan latihan penguatan menghasilkan dua
  item pada invoice kunjungan hari itu.
- **FR-RHB-034** — Sesi `NotDone` tidak pernah tertagih.
- **FR-RHB-035** — Ulang handoff bersifat idempoten. Item tidak pernah ganda.

### EPIC RHB-06 — Reasesmen dan penutupan (FEAT-024..029)
Disposisi: `MISSING / NEW`.
- **FR-RHB-040** — Pembanding beda instrumen wajib alasan.
- **FR-RHB-041** — Keputusan `Stop` menghentikan program terkait.
- **FR-RHB-050** — Ringkasan akhir ditolak selama ada catatan draft atau sesi berjalan.
- **FR-RHB-051** — Alasan penutupan membedakan selesai klinis, henti, rujuk, pindah, putus
  layanan, meninggal, dan menolak.
- **FR-RHB-052** — Episode tertutup tidak menerima dokumen baru.

### EPIC RHB-07 — Lintas: privasi dan anti-IDOR
Disposisi: `MISSING / NEW`.
- **FR-RHB-060** — Log tidak memuat data klinis.
- **FR-RHB-061** — Pembaca berkas RM rehab mengembalikan kosong untuk pasien yang tidak cocok.

## 11. Model status yang diusulkan

Lihat `contracts/state-transition-matrix.md`. Permintaan: `Draft → Submitted →
NeedsClarification / Accepted / Redirected / Rejected`, dan `Cancelled`. Episode:
`AwaitingMedicalAssessment → Planning → Active → Closed`. Program: `Draft → Active →
Superseded / Stopped / Completed`. Sesi: `InProgress → Completed / StoppedEarly`, `NotDone`.
Dokumen: `Draft → Final`. Invariant utamanya RHB-INV-01..10.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| Encounter, InpEpisode, pasien, dokter, profesi, tindakan, penomoran, keutuhan + addendum RM, mesin instrumen, adapter fakta charge | `ClinicalDocumentKind` (+6), `JenisYangDitegakkan` (+6), `ClinicalInstrumentKind` (+6), `PatientProcedureSource` (+1) | 20 tabel `Rhb*`, 16 service, 13 controller, 14 layar FE |

## 13. Sasaran kemampuan API

Seluruhnya bagian dari `contracts/api-contract.md` dan berstatus **Rencana (belum tersedia)**.

| Grup `[Tags]` | Endpoint inti | Epic |
|---|---|---|
| Rehab Request | `GET /`, `POST /`, `PUT /{id}`, `POST /{id}/submit|return|request-clarification|accept|redirect|reject|cancel`, `GET /duplicate-check` | `EPIC RHB-01` |
| Rehab Lookup | `GET /patients|encounters|internal-referral-letters|doctors|disciplines|disciplines/{id}/procedures|instruments` | `EPIC RHB-00` |
| Rehab Episode | `GET /`, `GET /{id}`, `GET /{id}/timeline` | `EPIC RHB-02..06` |
| Rehab Medical / Discipline Assessment | `POST /by-episode/{id}`, `PUT /{id}`, `POST /{id}/sign` | `EPIC RHB-02` |
| Rehab Goal / Program / Education | `POST /by-episode/{id}`, `POST /{id}/revise|activate|stop|sign` | `EPIC RHB-03` |
| Rehab Session | `POST /by-program/{id}/start|not-done`, `POST /{id}/complete|stop-early|sign` | `EPIC RHB-04` |
| Rehab Billing Handoff | `GET /{id}/billing-handoff`, `POST /{id}/billing-handoff/retry` | `EPIC RHB-05` |
| Rehab Progress Review / Discharge Summary | `POST /by-episode/{id}`, `POST /{id}/sign`; `PUT`/`POST sign` `/by-episode/{id}` | `EPIC RHB-06` |
| Rehab Discipline | 9 endpoint master + `/{id}/procedures`, `/{id}/instruments` | `EPIC RHB-00` |

## 14. Matriks kewenangan

Lihat `contracts/permission-audit-matrix.md` §2–3. String persis, contoh:
`[AccessPermission("RehabRequest", "Disposition")]`, `[AccessPermission("RehabSession", "Sign")]`,
`[AccessPermission("RehabBillingHandoff", "Retry")]`.

## 15. Batas integrasi dan billing

Modul ini **tidak boleh** membuat sendiri: master pasien, kunjungan/encounter, tarif, invoice,
item tagihan langsung, kunci tanda tangan, addendum, dan mesin skor instrumen. Tagihan hanya
lewat `TrxPatientProcedure` + `ClinicalMilestoneFactProducer` (`contracts/integration-contract.md`
INT-RHB-08/09). SEP, surat kontrol, klaim, dan SATUSEHAT berada di luar MVP.

## 16. Guardrail regulasi

| Kewajiban | Sumber | Penegakan MVP |
|---|---|---|
| Catatan lengkap, kronologis, bernama, berwaktu, bertanda tangan | Permenkes 24/2022 Ps. 16 | Validasi saat tanda tangan + keutuhan RM |
| Koreksi tanpa menghapus catatan asli | Permenkes 24/2022, KMK 1596/2024 | Addendum RM |
| Asesmen → rencana → intervensi → evaluasi | Permenkes 65/2015, 76/2014, 81/2014 | RHB-INV-03, alur status |
| Dokumentasi modifikasi layanan | Permenkes 81/2014 | Deviasi sesi |
| Privasi data pasien | Permenkes 24/2022 | Kolom S, log tanpa data klinis |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| NFR-001 | Atomicity: penerimaan permintaan + pembukaan episode, aktivasi program + supersede versi lama, tanda tangan sesi + tindakan bertarif masing-masing satu transaksi |
| NFR-002 | Concurrency: `Version` + `expectedVersion`. Bentrok dijawab 409. |
| NFR-003 | Idempotensi handoff: `IdempotencyKey = RHB-SESINT-{id}`. Ulang tidak menggandakan tagihan. |
| NFR-004 | Otorisasi di server untuk setiap endpoint, ditambah pemeriksaan penulis dan profesi |
| NFR-005 | Audit status lewat `RhbStatusHistory`. Log tanpa kolom sensitif. |
| NFR-006 | Waktu disimpan UTC (`timestamptz`) dan ditampilkan sesuai zona RS. Waktu tanda tangan dari server. |
| NFR-007 | Daftar: `AsNoTracking`, proyeksi DTO, `pageSize` maksimum 100 |

## 18. Skenario UAT

| ID | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| UAT-01 | RHB-01 | Pasien punya kunjungan Poli Saraf hari ini | Admisi membuat dan mengajukan permintaan fisioterapi | Permintaan `Submitted` bernomor `RHB-REQ-…` |
| UAT-02 | RHB-01 (gagal) | Pasien punya episode fisioterapi aktif | Ajukan permintaan fisioterapi baru tanpa alasan | Ditolak, episode lama ditampilkan. Dengan alasan, berhasil. |
| UAT-03 | RHB-01 (gagal) | Petugas admisi tanpa butir disposisi | Coba menolak permintaan | Tombol tidak ada. Panggilan langsung dijawab 403. |
| UAT-04 | RHB-02 | Episode baru | Sp.KFR mengisi asesmen + resep fisio & wicara, lalu menandatangani | Episode `Planning`, dua layanan diresepkan |
| UAT-05 | RHB-02 (gagal) | Asesmen final | Coba mengubah isi | Ditolak. Addendum berhasil, dan catatan asli tetap terlihat. |
| UAT-06 | RHB-03 (gagal) | Asesmen fisio belum final | Aktifkan program fisio | Ditolak dengan daftar prasyarat |
| UAT-07 | RHB-03 | Prasyarat lengkap, edukasi disetujui | Aktifkan program | Program `Active`, episode `Active` |
| UAT-08 | RHB-04/05 | Program aktif, pasien datang, kunjungan terdaftar | Cek keselamatan → 2 tindakan → selesai → tanda tangan | Sesi final, 2 item tagihan di invoice kunjungan |
| UAT-09 | RHB-04 (gagal) | Kunjungan kemarin sudah ditutup | Mulai sesi memakai kunjungan itu | Ditolak "kunjungan sudah berakhir" |
| UAT-10 | RHB-04/05 (gagal) | Pasien tidak datang | Catat tidak terlaksana, tanda tangan | Tidak ada tagihan |
| UAT-11 | RHB-05 (gagal) | Tindakan belum bertarif | Tanda tangan sesi | Handoff `Failed`. Setelah tarif dibetulkan dan Ulang ditekan dua kali, hanya satu item tagihan. |
| UAT-12 | RHB-04 (gagal) | Pasien pusing di tengah sesi | Henti dini tanpa tindakan segera | Ditolak. Dengan tindakan segera, sesi `StoppedEarly` dan program "perlu telaah". |
| UAT-13 | RHB-06 | 8 sesi final, tujuan tercapai | Reasesmen `Complete`, lalu ringkasan akhir ditandatangani | Episode `Closed` |
| UAT-14 | RHB-06 (gagal) | Ada sesi `InProgress` | Tanda tangan ringkasan akhir | Ditolak dengan daftar catatan yang belum final |
| UAT-15 | RHB-00 (gagal) | Pengguna hanya punya `RehabRequest:Read` | Buka URL episode | Layar akses ditolak |
| UAT-16 | RHB-07 | Selesai UAT-01..14 | Periksa log aplikasi | Tidak ada indikasi/temuan/respons di log |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Satu pasien berjalan dari permintaan sampai episode `Closed` | UAT-01, 04, 07, 08, 13 |
| Penolakan klinis hanya oleh pemegang disposisi | UAT-03 |
| Program tidak aktif tanpa asesmen final | UAT-06 |
| Catatan final tidak berubah; koreksi lewat addendum | UAT-05 |
| Sesi terlaksana tertagih tepat sekali; `NotDone` tidak tertagih | UAT-08, 10, 11 |
| Episode tidak tertutup selama ada draft | UAT-14 |
| Akses ditolak di server dan layar | UAT-03, 15 |
| Log bebas data klinis | UAT-16 |
| Seluruh layar terjangkau dari menu atau layar induk | `03-frontend-architecture.md` §2 + UAT-15 |
| Data master go-live terisi: poli rehab, profesi OT/TW, tindakan rehab bertarif, `RhbDiscipline` berprofesi, instrumen disahkan | `02-backend-architecture.md` §8, RHB-DEC-020, OQ-016 |
| Migration diterapkan, build sukses, tanpa perubahan model tertunda (dijalankan owner) | Laporan task migration |
| Bukti persetujuan RM dan izin Cli/Billing terlampir | OQ-012, OQ-015 |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | Fondasi: model + migration, konfigurasi disiplin, konstanta hak akses, lookup, enum RM/Cli, menu | Blueprint disetujui |
| `MVP-1` | EPIC RHB-01 permintaan + disposisi (BE+FE) | MVP-0 |
| `MVP-2` | EPIC RHB-02, 03 asesmen, tujuan, program, edukasi | MVP-1; bukti OQ-012 untuk ✅ |
| `MVP-3` | EPIC RHB-04, 05 sesi + handoff billing | MVP-2; OQ-015 |
| `MVP-4` | EPIC RHB-06, 07 reasesmen, penutupan, berkas RM, privasi | MVP-3 |
| `POST-MVP` | FEAT-004..006, 011, 014, 019..023, 030, 031, 033; OQ-013, OQ-014 | Keputusan UNR-005..008 |

| Pertanyaan | Memblokir? |
|---|---|
| RHB-OQ-012 bukti persetujuan RM | Memblokir ✅ task yang menyentuh berkas RM, bukan development lock |
| RHB-OQ-015 izin `TrxPatientProcedure` + fakta charge | Memblokir task handoff (MVP-3) |
| RHB-OQ-016 tindakan rehab bertarif di master | Memblokir go-live, bukan development |
| RHB-OQ-001 nama pengesah klinis | Memblokir aktivasi produksi |
| RHB-OQ-005 definisi putus layanan | Tidak memblokir |
