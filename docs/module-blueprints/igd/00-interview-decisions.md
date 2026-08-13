# IGD — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `IGD-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Interview mode | `Scope pass` |
| Product/domain owner | `OPEN — belum diidentifikasi` |
| Backend SHA | `pending-trace` |
| Frontend SHA | `pending-trace` |
| Primary evidence | `docs/Modul-RS/Alur_Bisnis_Modul_IGD_NewQuilvian.md`, versi 1.0, 5 Agustus 2026 |

## Scope dan Outcome

- **Fact:** Dokumen sumber mencakup kedatangan, triage/retriage, registrasi, pelayanan
  klinis, observasi, resusitasi, transfer, disposition, billing/administrasi, dan penutupan
  kunjungan IGD.
- **Fact:** Outcome yang dinyatakan adalah satu episode IGD yang dapat ditelusuri melalui
  `EncounterId`, tanpa menduplikasi data klinis umum dan tanpa menghambat pasien gawat.
- **Assumption:** Blueprint bernama canonical `igd` dan dokumen sumber menjadi input Scope
  Pass, bukan bukti approval final.
- **Assumption:** Audit capability backend dan frontend belum dilakukan; nama class, tabel,
  konfigurasi, dan integrasi di dokumen sumber masih harus dibuktikan dari source code.
- **Decision (draft):** Mode korban massal/bencana termasuk scope rilis awal modul IGD.

## Glossary

| Istilah | Makna kerja saat ini | Status |
|---|---|---|
| Episode IGD | Satu rangkaian pelayanan IGD yang berpusat pada satu `TrxPatientEncounter` dan satu `TrxEmergencyVisit` aktif | `draft` |
| Encounter provisional | Encounter dengan identitas/administrasi minimum yang boleh dipakai agar pelayanan gawat dapat langsung dicatat | `draft`; kriteria minimum dan batas waktunya terbuka |
| Registrasi lengkap | Administrasi pasien dan penjamin telah memenuhi syarat yang belum didefinisikan secara teruji | `OPEN` |
| Stabil | Kondisi yang mengizinkan pelengkapan administrasi setelah pelayanan segera; kriterianya belum didefinisikan | `OPEN` |
| Disposition | Keputusan klinis dokter mengenai kelanjutan pasien | `draft` |
| Transfer | Eksekusi perpindahan pasien, terpisah dari keputusan disposition | `draft` |
| Selesai/Completed | Disposition telah dieksekusi dan seluruh gate penutupan yang berlaku terpenuhi | `draft`; gate detail terbuka |

## Aktor dan Tanggung Jawab

| Aktor | Tanggung jawab yang dinyatakan dokumen | Status |
|---|---|---|
| Petugas penerimaan/registrasi | Identifikasi awal, membuat atau melengkapi encounter, penjamin, penanggung jawab, dan rujukan | `draft` |
| Perawat triage | Triage awal, ABCDE/red flag, level prioritas, dan retriage | `draft` |
| Dokter IGD | SOAP/konsultasi, diagnosis, tindakan, serta menetapkan disposition | `draft` |
| Perawat IGD | Monitoring, CPPT, observasi, edukasi, dan handover sesuai kewenangan klinis | `draft` |
| Tim/unit tujuan | Menerima atau menolak transfer dan mengonfirmasi kedatangan | `draft` |
| Billing/administrasi | Finalisasi biaya dan kekurangan administrasi sebelum penutupan sesuai kebijakan | `draft` |
| Product/domain owner | Memutus scope dan aturan bisnis IGD | `OPEN — orang/role belum ditetapkan` |
| Clinical governance owner | Menyetujui sistem triage, target respons, override, dan aturan keselamatan klinis | `OPEN — orang/role belum ditetapkan` |
| Security/privacy owner | Menyetujui akses, audit, data pasien tak dikenal, serta retention | `OPEN — orang/role belum ditetapkan` |
| Kepala IGD | Mengaktifkan mode korban massal/bencana dalam status menunggu konfirmasi; tidak boleh menonaktifkan kecuali resmi merangkap incident commander | `draft` |
| Incident commander | Mengaktifkan langsung sebagai terkonfirmasi dan dapat menonaktifkan mode korban massal/bencana | `draft` |
| Direktur rumah sakit | Mengonfirmasi/menolak aktivasi, menerima notifikasi, dan dapat menonaktifkan mode | `draft` |
| Pejabat/komite bencana yang ditunjuk | Mengonfirmasi/menolak aktivasi dan menerima notifikasi sesuai penunjukan resmi | `draft` |

## Business Rules dan Invariants

Pernyataan berikut berasal dari dokumen sumber dan dicatat sebagai keputusan **draft**, belum
sebagai fakta source code atau approval manusia:

1. Encounter IGD menggunakan tipe `OP`, asal kunjungan IGD, dan `ServiceUnitId` IGD.
2. `TrxPatientEncounter` menjadi pusat episode; `TrxEmergencyVisit` menjadi extension IGD.
3. Satu `EncounterId` hanya boleh memiliki satu `TrxEmergencyVisit` aktif.
4. Pelengkapan administrasi provisional tidak boleh membuat encounter kedua.
5. Pasien level 1–2 dapat dilayani dengan encounter provisional dan identitas minimum.
6. Data klinis umum dimiliki Clinical Management; data yang unik bagi IGD dimiliki
   Emergency Installation Management.
7. Retriage adalah transaksi baru pada visit yang sama dan tidak menimpa histori triage.
8. Tindakan utama tetap menjadi `TrxPatientProcedure`; atribut khusus IGD menjadi extension.
9. Observation/resuscitation tidak boleh selesai sebelum waktu mulai.
10. Disposition dan transfer adalah konsep terpisah.
11. Visit hanya dapat ditutup setelah disposition dieksekusi dan gate yang relevan terpenuhi.
12. Data klinis memakai soft delete dan audit trail.
13. Log mutasi tidak boleh membawa identitas pasien atau payload klinis lengkap.
14. Arah triase yang dipilih adalah kategori warna Indonesia: Merah, Kuning, Hijau, dan
    Hitam. Permenkes 47/2018 menerapkan kategori ini pada pelayanan IGD rumah sakit;
    kejadian bencana/korban massal menambahkan tag triase dan tata kelola krisis.
15. Kategori Hitam tidak boleh ditentukan otomatis oleh aplikasi. Dokumen regulasi
    mendefinisikannya sebagai pasien meninggal atau cedera fatal yang jelas dan tidak mungkin
    diresusitasi; penetapan pada pasien konkret memerlukan tenaga klinis berwenang, alasan,
    waktu, dan audit trail.
16. Aktivasi oleh Kepala IGD menghasilkan status `Active — Pending Confirmation` agar respons
    klinis dan operasional tidak menunggu administrasi.
17. Aktivasi oleh incident commander yang telah ditetapkan resmi menghasilkan status
    `Active — Confirmed`; direktur dan komite bencana tetap menerima notifikasi.
18. Penonaktifan hanya boleh dilakukan incident commander atau direktur. Kepala IGD hanya
    boleh menonaktifkan apabila pada saat itu juga ditetapkan resmi sebagai incident commander.
19. Aktivasi, konfirmasi, penolakan konfirmasi, dan penonaktifan wajib mencatat identitas dan
    jabatan pelaku, waktu server, alasan, jenis/lokasi kejadian, sumber laporan, referensi
    bukti, serta status sebelum dan sesudah tindakan.
20. Lampiran bukti tidak boleh menjadi prasyarat aktivasi. Alasan dan sumber laporan wajib
    saat aktivasi; dokumen/foto/bukti tambahan boleh dilengkapi setelah mode aktif.
21. Bukti sensitif disimpan pada penyimpanan terbatas. Audit log hanya menyimpan ID/referensi
    bukti dan tidak boleh memuat identitas pasien atau payload klinis lengkap.
22. Batas waktu konfirmasi belum boleh di-hardcode sampai SOP internal MMC Hospital atau
    Hospital Disaster Plan diverifikasi.

## State dan Transition

State utama yang tersirat pada dokumen sumber:

`Provisional → Registered → In Service → Disposition → Completed`

Transfer rawat inap yang dinyatakan:

`Requested → Accepted → Departed → Arrived`

State aktivasi mode korban massal/bencana yang sudah diputuskan secara draft:

- Kepala IGD: `Inactive → Active — Pending Confirmation`.
- Incident commander resmi: `Inactive → Active — Confirmed`.
- `Active — Pending Confirmation → Active — Confirmed` melalui konfirmasi direktur,
  incident commander, atau pejabat/komite bencana yang ditunjuk.
- `Active → Deactivated` hanya oleh incident commander atau direktur.
- Transisi setelah `Confirmation Rejected` masih terbuka pada `IGD-OQ-014`.

Hal yang belum ditentukan:

- legal transition, guard, actor, dan timestamp untuk setiap perpindahan state;
- perilaku penolakan transfer, pembatalan, koreksi, reopening, dan duplicate command;
- apakah `Registered` selalu mengikuti `Provisional` atau keduanya alternatif initial state;
- sinkronisasi status emergency visit, encounter, billing, admission, dan transfer;
- behavior ketika dependency lintas modul unavailable atau hanya sebagian berhasil.

## Skenario Normal dan Exception

Skenario awal dari dokumen sumber:

- pasien lama level 4 melalui registrasi standar;
- pasien tak dikenal level 1 melalui temporary patient dan provisional encounter;
- kondisi memburuk dari level 3 ke level 2 melalui retriage tanpa menghapus histori;
- observasi berkala mereferensikan vital sign tanpa duplikasi;
- rawat inap mensyaratkan admission/unit tujuan dan transfer sampai `Arrived`;
- pulang mensyaratkan instruksi, resep, billing, dan administrasi;
- PAPS mensyaratkan alasan, consent/saksi, kondisi keluar, instruksi keselamatan, dan audit;
- pembuatan emergency visit aktif kedua ditolak.

Skenario yang masih terbuka:

- pasien datang kembali setelah visit selesai;
- salah identitas dan merge temporary patient;
- koreksi triage/disposition tanpa merusak audit trail;
- pembatalan atau perubahan disposition;
- pasien meninggal sebelum identitas/administrasi lengkap;
- pasien kabur saat billing, resep, atau hasil penunjang belum selesai;
- unit rawat inap menolak atau membatalkan penerimaan setelah sebelumnya menerima;
- downtime/offline, retry, duplicate submit, timeout, serta partial failure antar modul;
- hasil lab/radiologi datang setelah pasien meninggalkan IGD;
- penutupan visit ketika billing belum final tetapi pasien secara klinis sudah boleh keluar.

## Frontend Decision Authority

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `IGD-UI-001` | Privacy dan masking data klinis/identitas | Security/privacy owner | `open` | Harus mengikuti klasifikasi data dan permission approved | Belum tersedia |
| `IGD-UI-002` | Siapa boleh melihat/mengubah state klinis dan administratif | Product/domain + security owner | `open` | Tidak boleh ditentukan oleh frontend | Belum tersedia |
| `IGD-UI-003` | Peringatan SLA triage dan eskalasi | Clinical governance + product owner | `open` | Severity dan tindakan harus berasal dari rule approved | Dokumen §5 belum menetapkan escalation policy |
| `IGD-UI-004` | Menu, route, bentuk page/tab/modal/drawer, layout, dan warna presentasi | Frontend authority | `DEV_DISCRETION` setelah constraint approved | Ikuti project convention dan accessibility; warna klinis tidak boleh mengubah arti level | Belum diaudit |
| `IGD-UI-005` | Kontrol aktivasi, konfirmasi, penolakan, dan penonaktifan mode bencana | Product/domain + security owner | `draft` | UI wajib mengikuti role dan legal transition; tidak boleh didelegasikan ke developer | Jawaban user 13 Agustus 2026 |

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `IGD-DEC-001` | Decision | Gunakan satu encounter OP asal/unit IGD untuk satu episode, dengan emergency visit sebagai extension | Product/domain owner + Registration API owner | `draft` | — | Dokumen §1–4 dan §9 |
| `IGD-DEC-002` | Decision | Pasien gawat boleh memakai provisional encounter tanpa menunggu administrasi lengkap | Product/domain + clinical governance owner | `draft` | — | Dokumen §4.2 |
| `IGD-DEC-003` | Decision | Data klinis umum tidak diduplikasi di modul IGD | Product/domain + data/API owners | `draft` | — | Dokumen §1, §2, dan §6 |
| `IGD-DEC-004` | Decision | Retriage append-only dan mereferensikan histori sebelumnya | Clinical governance owner | `draft` | — | Dokumen §5.2 |
| `IGD-DEC-005` | Decision | Disposition terpisah dari eksekusi transfer | Product/domain + clinical governance owner | `draft` | — | Dokumen §7.3–8 |
| `IGD-DEC-006` | Decision | Mutation logging tidak menyimpan PHI/payload klinis lengkap | Security/privacy owner | `draft` | — | Dokumen §9.3 |
| `IGD-OQ-001` | Conflict | Sistem triage disebut “ATS atau ESI”, tetapi level, warna, target waktu, dan fast-track digabung seolah satu skema | Clinical governance owner | `superseded` oleh `IGD-DEC-007` | — | Dokumen §5; jawaban user 13 Agustus 2026 |
| `IGD-OQ-002` | Open Question | Siapa product/domain owner dan siapa approver final keputusan kritis? | Sponsor modul | `open` | — | Tidak tercantum pada dokumen |
| `IGD-OQ-003` | Open Question | Apa field minimum provisional encounter dan kapan administrasi dianggap lengkap/stabil? | Registration + product/domain owner | `open` | — | Dokumen §4.2 memakai istilah tanpa kondisi teruji |
| `IGD-OQ-004` | Open Question | Bagaimana lifecycle, merge, deduplication, dan audit temporary patient? | Master Patient owner + privacy owner | `open` | — | Dokumen §4.3 hanya menyebut rekonsiliasi sesuai kebijakan |
| `IGD-OQ-005` | Open Question | Apa legal transition dan permission untuk visit, disposition, transfer, correction, cancellation, dan reopening? | Product/domain + clinical governance owner | `open` | — | Dokumen §7–9 belum lengkap |
| `IGD-OQ-006` | Open Question | Apakah billing harus benar-benar final untuk close, atau boleh outstanding/ditagihkan dengan reason dan owner? | Finance/billing + product owner | `open` | — | Dokumen §8 menyebut “selesai/ditindaklanjuti” |
| `IGD-OQ-007` | Open Question | Bagaimana aturan hasil penunjang terlambat dan clinical follow-up setelah pasien keluar? | Clinical governance owner | `open` | — | Belum dibahas |
| `IGD-OQ-008` | Open Question | Apa failure/retry/idempotency policy untuk transaksi lintas Registration, Clinical, Pharmacy, Lab/Radiology, Billing, dan Inpatient? | API/data owners | `open` | — | Belum dibahas |
| `IGD-OQ-009` | Open Question | Permission matrix dan separation of duties per aktor belum ditentukan | Product/domain + security owner | `open` | — | Dokumen hanya menyebut atribut access controller |
| `IGD-OQ-010` | Open Question | Apakah target response adalah SLA ke triage, kontak klinis pertama, atau intervensi; dari timestamp mana dihitung dan apa eskalasinya? | Clinical governance owner | `open` | — | Dokumen §5 hanya memberi angka target |
| `IGD-DEC-007` | Decision | Gunakan sistem triase warna Indonesia: Merah, Kuning, Hijau, dan Hitam; jangan memakai label ATS/ESI sebagai source of truth | Clinical governance owner | `draft` | — | Jawaban user 13 Agustus 2026; Permenkes 47/2018 |
| `IGD-OQ-011` | Open Question | Apakah satu skema empat warna berlaku sama untuk IGD harian dan bencana/mass-casualty, khususnya penggunaan kategori Hitam? | Clinical governance + product/domain owner | `superseded` oleh `IGD-FACT-001` dan `IGD-OQ-012` | — | Permenkes 47/2018 dan Kepmenkes 1502/2023 memberi jawaban normatif yang lebih tepat |
| `IGD-FACT-001` | Fact | Empat kategori warna berlaku pada pelayanan IGD; bencana/korban massal menambahkan tag triase, manajemen korban massal, dan organisasi krisis. Status harus dinilai ulang dan perubahan dicatat sebagai retriase | — | `approved` | Kemenkes evidence verified/13 Agustus 2026 | Permenkes 47/2018; Permenkes 1/2026; Kepmenkes 1502/2023 |
| `IGD-OQ-012` | Open Question | Apakah disaster/mass-casualty mode termasuk scope rilis awal modul IGD atau menjadi fase terpisah setelah alur IGD harian? | Product/domain owner | `superseded` oleh `IGD-DEC-009` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-008` | Decision | Pada korban massal/bencana, korban tetap diklasifikasikan dengan empat warna berdasarkan tingkat kegawatdaruratan; perubahan kategori dilakukan melalui retriase dan histori tidak ditimpa | Clinical governance + product/domain owner | `draft` | — | Jawaban user 13 Agustus 2026; Permenkes 47/2018; Kepmenkes 1502/2023 |
| `IGD-DEC-009` | Decision | Mode korban massal/bencana termasuk scope rilis awal modul IGD | Product/domain owner | `draft` | — | Jawaban user “ya”, 13 Agustus 2026; owner/approver belum diidentifikasi |
| `IGD-OQ-013` | Open Question | Siapa yang boleh mengaktifkan dan menonaktifkan mode korban massal/bencana di rumah sakit, termasuk aktivasi sementara sebelum status eksternal diterbitkan? | Product/domain + clinical governance + security owner | `superseded` oleh `IGD-DEC-010` sampai `IGD-DEC-013` | — | Jawaban user 13 Agustus 2026 |
| `IGD-DEC-010` | Decision | Kepala IGD mengaktifkan sebagai `Active — Pending Confirmation`; incident commander resmi mengaktifkan sebagai `Active — Confirmed` | Product/domain + clinical governance owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 1–3 |
| `IGD-DEC-011` | Decision | Direktur, incident commander, atau pejabat/komite bencana yang ditunjuk dapat mengonfirmasi; aktivasi terkonfirmasi oleh incident commander tetap menotifikasi direktur dan komite | Product/domain + security owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 2–3 |
| `IGD-DEC-012` | Decision | Hanya incident commander atau direktur yang dapat menonaktifkan; Kepala IGD hanya bila resmi merangkap incident commander | Product/domain + security owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 4 |
| `IGD-DEC-013` | Decision | Semua perubahan state memakai waktu server dan audit lengkap; bukti boleh menyusul, disimpan terbatas, dan audit hanya menyimpan referensi tanpa PHI/payload klinis | Security/privacy + product/domain owner | `draft` | User stated approval/13 Agustus 2026; authority unverified | Jawaban user butir 5–7 |
| `IGD-OQ-014` | Open Question | Apa dampak penolakan konfirmasi terhadap mode dan pelayanan korban yang sudah berjalan? | Clinical governance + incident command + product/domain owner | `open` | — | Penolakan tidak boleh menghentikan pelayanan secara tidak aman atau menghapus histori |
| `IGD-OQ-015` | Open Question | Berapa batas waktu konfirmasi dan eskalasinya? | MMC Hospital Disaster Plan/SOP owner | `open` | — | Menunggu verifikasi SOP internal MMC Hospital atau Hospital Disaster Plan |

## Evidence Eksternal Indonesia

- [Permenkes Nomor 1 Tahun 2026 tentang KLB, Wabah, dan Krisis Kesehatan](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-1-tahun-2026)
  adalah payung terkini dan berlaku sejak 21 Januari 2026. Regulasi ini mencabut Permenkes
  75/2019 dan Permenkes 19/2016; ketentuan SPGDT kini berada di Permenkes 1/2026.
- [Permenkes Nomor 47 Tahun 2018 tentang Pelayanan Kegawatdaruratan](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-47-tahun-2018)
  tercatat tetap berlaku pada JDIH Kementerian Kesehatan per 13 Agustus 2026. Regulasi ini
  menetapkan empat warna untuk triase IGD, retriase berkelanjutan, dan tag triase ketika
  menangani bencana/korban dalam jumlah banyak.
- [Kepmenkes HK.01.07/Menkes/1502/2023 tentang Pedoman Nasional Penanggulangan Krisis Kesehatan](https://jdih.kemkes.go.id/documents/keputusan-menteri-kesehatan-nomor-hk0107menkes15022023)
  tercatat tetap berlaku dan menjadi pedoman operasional klaster kesehatan, EMT, triase,
  surge, rujukan, data/surveilans, serta penanganan lintas sub-klaster.
- [Pedoman Nasional Penanggulangan Krisis Kesehatan](https://pusatkrisis.kemkes.go.id/__pub/files49279Final_Pedoman_Nasional_Penanggulangan_Krisis_Kesehatan.pdf)
  mendefinisikan kategori Merah, Kuning, Hijau, dan Hitam berdasarkan prioritas serta ABCDE
  dalam konteks krisis/bencana dan triase di fasilitas kesehatan.
- [Artikel Direktorat Jenderal Kesehatan Lanjutan mengenai IGD RSUP Dr. Sardjito](https://keslan.kemkes.go.id/view_artikel/1081/ruang-resusitasi-igd-rsup-dr-sardjito-yogyakarta-untuk-pelayanan-lebih-baik)
  mendeskripsikan penggunaan empat warna pada pemilahan pasien IGD.
- [Playbook SATUSEHAT Pelayanan IGD](https://satusehat.kemkes.go.id/platform/docs/id/interoperability/igd/)
  menjadi evidence awal bahwa data triase terhubung ke satu `Encounter`; pemetaan kontrak
  final tetap menunggu capability audit.

## Acceptance Criteria

Acceptance criteria awal dari dokumen sumber, seluruhnya berstatus `draft` sampai owner dan
rule terkait disetujui:

1. Sistem menolak emergency visit aktif kedua pada encounter yang sama.
2. Pelengkapan encounter provisional mempertahankan `EncounterId` dan seluruh relasi klinis.
3. Pasien level cepat dapat memperoleh tindakan/resusitasi tanpa menunggu administrasi
   lengkap, tetapi setiap transaksi tetap tertaut ke encounter.
4. Retriage membuat record baru dan mempertahankan record sebelumnya.
5. Observation detail dapat mereferensikan vital sign/CPPT tanpa menyalin data klinis umum.
6. Rawat inap tidak menutup visit sebelum admission, handover, dan transfer mencapai state
   yang disetujui.
7. PAPS menyimpan alasan, bukti consent/saksi, kondisi saat keluar, instruksi keselamatan,
   dan audit.
8. Semua mutasi penting dapat diaudit tanpa mengekspos PHI pada payload log.
9. Dalam mode korban massal/bencana, setiap korban memiliki hasil triase empat warna yang
   tertaut pada konteks insiden; retriase membuat histori baru tanpa menimpa hasil sebelumnya.
10. Aktivasi oleh Kepala IGD langsung berhasil tanpa lampiran dan menghasilkan `Active —
    Pending Confirmation`, sedangkan incident commander resmi menghasilkan `Active —
    Confirmed` serta notifikasi kepada direktur dan komite.
11. Kepala IGD yang tidak merangkap incident commander ditolak ketika mencoba menonaktifkan
    mode; incident commander dan direktur dapat menonaktifkannya.
12. Setiap perubahan state menggunakan waktu server dan menyimpan seluruh field audit yang
    diputuskan; audit log tidak memuat PHI atau payload klinis lengkap.
13. Bukti tambahan dapat ditautkan setelah aktivasi tanpa mengubah histori aktivasi awal dan
    hanya dapat diakses oleh role yang berwenang.

## Open Questions dan Blocker

### Pertanyaan aktif — jawab satu per giliran

`IGD-OQ-014`: Jika direktur/incident commander/komite **menolak konfirmasi** ketika penanganan
korban sudah berjalan, apakah mode tetap `Active — Pending Confirmation` dengan tanda
`Confirmation Rejected` sampai incident commander atau direktur melakukan penonaktifan
eksplisit? Model ini mencegah penolakan administratif menghentikan workflow klinis secara
mendadak; penolakan dan penonaktifan tetap menjadi dua audit event terpisah.

### Blocker desain saat ini

- Kewenangan aktivasi/penonaktifan ditutup secara draft melalui `IGD-DEC-010` sampai
  `IGD-DEC-013`; behavior penolakan konfirmasi masih diblokir oleh `IGD-OQ-014`.
- SLA konfirmasi tetap terbuka pada `IGD-OQ-015` sampai SOP internal diverifikasi.
- Approval semua keputusan material diblokir sampai owner/approver pada `IGD-OQ-002`
  diidentifikasi.
- Desain final state machine dan integrasi lintas modul diblokir oleh `IGD-OQ-003` sampai
  `IGD-OQ-010`, tetapi audit capability source code masih dapat dilakukan setelah batas Scope
  Pass cukup.
