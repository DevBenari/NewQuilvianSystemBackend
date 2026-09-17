# Laboratorium — Blueprint Manifest

```yaml
blueprint_id: LAB-BP-001
module_name: Laboratorium
module_slug: laboratorium
module_prefix: LAB
revision: 43
status: approved-with-pending-reconciliation   # rev 36: empat dari tujuh hal yang dibuka putaran 3 DITUTUP pemilik modul pada hari yang sama lewat LAB-DEC-070..073 — bentuk menu (tiga daftar sejajar bertahan, artifact tidak diadopsi apa adanya), pembanding tanggal inklusif, nomor order berupa kolom baru nomor urut terbaca, dan cakupan baris hanya pesanan Laboratorium. Tiga sisanya BUKAN wewenang pemilik modul dan diajukan lewat LAB-REQ-007 bersama LAB-COORD-011. Seluruh isi BR-50 tetap slice S17 dan tetap tertahan LAB-SIGN-001. rev 35: bukti putaran 3 (Menu Hasil, `LAB-EVD-002`) PARTIALLY_RECONCILED, enam klarifikasi diadopsi LAB-DEC-064..069 dan ditulis BR-50. rev 33: bukti putaran 2 RECONCILED lewat LAB-DEC-060..063. Menuntut amandemen LAB-STATE-v1, LAB-VAL-v1, LAB-API-v1 dan satu migration empat kolom. Sisa penahan LAB-COORD-006, LAB-COORD-007, LAB-COORD-010, LAB-OPEN-025, LAB-OPEN-026
bentuk: SINGLE
created_at: 2026-09-01T00:00:00+07:00
updated_at: 2026-09-17T00:00:00+07:00

scope:
  release: MVP Rilis 1 — bagian yang sudah lolos kedua gerbang
  slices_in_scope: [S1a, S2, S3, S7, S10, S11, S13a, S13b, S14, S15]
  slices_out_of_scope: [S1b, S2b, S4, S4b, S4c, S5, S6, S8, S9, S16, S17, S18, S19]
  discipline: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi (LAB-DEC-025). Bank Darah di luar scope

owners:
  product_domain: Yoga Aji Pratama <yogaaji452@gmail.com>
  api: belum ditetapkan
  security: belum ditetapkan
  frontend_authority: konvensi project + DEV_DISCRETION (LAB-DEC-010)
  clinical_governance: belum ditetapkan — lihat LAB-SIGN-001. Permintaan penetapan sekaligus tanda tangan diajukan 2026-09-09 lewat LAB-REQ-004

approved_by: Yoga Aji Pratama <yogaaji452@gmail.com>
approved_at: 2026-09-01

backend_commit_sha: "13665452"  # disegarkan 2026-09-16 oleh LAB-RDY-001. Naik dari e2152709 sejauh 23 commit, TETAPI hanya SATU di antaranya menyentuh source Laboratorium: a6e74742 "updates BE modul lab" (11 berkas). Dua puluh dua sisanya milik modul lain dan merge dari origin/QuilvianIntegrationBackend. Karena itu bukti capability map TIDAK basi oleh pekerjaan modul lain, dan impact scan penuh tidak dijalankan ulang — cakupannya sudah dibuktikan 06-readiness-assessment.md bagian 3.5
backend_commit_sha_previous: "e2152709"   # titik pindai sampai revision 38
backend_impact_scan:
  from: "9124900"
  to: "c87d9c0"
  dilakukan: 2026-09-01
  temuan_berdampak:
    - ClinicalBillingIntegration dipindah ke ClinicalManagement
    - TrxClinicalMilestoneFact dinamai ulang menjadi CliClinicalMilestoneFact (migration RenameClinicalMilestoneFactToCliPrefix)
    - Configuration Laboratorium dipindah ke Repositories/Configurations/HealthServices/LaboratoryManagement/
    - Berkas uji dipindah dari tests/ ke Tests/
  tidak_berdampak:
    - Model Laboratorium tidak berubah; temuan capability map atas model tetap sahih
    - Perubahan LabOrderService dan LabSpecimenService hanya pada baris using
frontend_commit_sha: "686038858"  # disegarkan 2026-09-16 oleh LAB-RDY-001. Naik dari 9cd4cd03f sejauh 21 commit; TIGA menyentuh Laboratorium (686038858, 2e3496258, 4031fd3d7 — seluruhnya "updates FE modul lab", 31 berkas). Uji unit Laboratorium dijalankan ulang pada SHA ini: 158/158 lulus
frontend_commit_sha_previous: "9cd4cd03f"

input_revisions:                # diperbarui 2026-09-14, menutup LAB-OPEN-023
  decisions: 36   # rev 36 — riwayat revisi dirapikan: baris 32 disusun ulang dari manifest dan dokumen rekonsiliasi, baris 31 ditandai tidak dapat dipulihkan. rev 35 — BR-48 dan BR-49 yang hilang ditulis, disusun ulang dari keputusan dan kontrak terkunci; nol aturan baru. rev 34 — LAB-DEC-070..073 menutup LAB-CONFLICT-009, LAB-OPEN-028, LAB-OPEN-031, dan LAB-OPEN-033 pada hari yang sama. rev 33 — LAB-DEC-064..069 menutup enam klarifikasi Menu Hasil dan menulis BR-50; LAB-EVD-002 diterima. rev 32 — LAB-DEC-060..063 menutup rekonsiliasi bukti putaran 2; rev 30 persetujuan Andry Zain lewat LAB-REQ-006; keempat keputusan kiosk naik menjadi approved
  capability_map: 3   # impact scan 2026-09-14 atas BE 466a7127 + FE 9cd4cd03f. Sebagian besar peta revision 1-2 STALE; F5 dicabut
  requirement_gate: LAB-RCG-001-r4
  domain_architecture: LAB-DA-001-r4

input_hashes:                  # sha256 penuh atas isi ber-line-ending LF, mengikuti konvensi pharmacy dan billing-kasir. Dihitung ulang 2026-09-14
  00-interview-decisions.md: 41b0e3a05087138294eecea194c055c60aa8109cdfa89e9b5b42f82a3dbfa3cb   # dihitung ulang 2026-09-16 sesudah revision 37; nilai lama a93c7823... berlaku sampai revision 32
  01-existing-capability-map.md: f946a02a512f01d6de5bda0240158e216b913fa5081ad0f3975e56113b15c963
  02-requirement-completeness-assessment.md: 3de86c8242a313a5a864a1eaa1cfffdb21149658789f01095d0ec847a9c072d1
  03-domain-architecture.md: 3279c0ef2309b52feab77d870f782f4ce02134b2457fa46bfd09d868d98493de
  # Kedua hash terakhir diverifikasi 2026-09-14 TIDAK berubah sejak 2026-09-02.
input_hashes_method: |
  sha256 atas isi berkas dengan line ending LF — sama dengan isi blob yang disimpan Git.
  Perintah: tr -d '\r' < <berkas> | sha256sum
  Konvensi diambil dari pharmacy/blueprint-manifest.md dan billing-kasir/blueprint-manifest.md,
  dan diverifikasi cocok dengan keempat artifact_hashes pharmacy pada 2026-09-02.
  Nilai revision 14 sampai 17 keliru: hanya 16 digit heksadesimal dan tidak cocok dengan isi
  berkas mana pun. Seluruhnya diganti, bukan dipotong ulang.

requirement_gate_readiness: PARTIALLY_READY
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
domain_architecture_revision: LAB-DA-001-r4

contract_versions:            # r3 dan sebelumnya dikunci 2026-09-02; amandemen 2026-09-14 disetujui Yoga Aji Pratama selaku pemilik modul
  - LAB-API-v1: approved      # revision 17 — r17 (GET /lab-specimens: daftar penerimaan lintas pesanan, disaring pada waktu kedatangan sebenarnya) disetujui pemilik modul 2026-09-17 dan dilaksanakan BE-LAB-38. r16 (orderNumber pada LabOrderListResponse dan LabMonitoringItemResponse, plus collectedAt opsional pada PlanLabSpecimenRequest) disetujui pemilik modul 2026-09-17 dan dilaksanakan BE-LAB-37. r16 memuat DUA isi dari dua tempat: usulan BE-LAB-36, dan LAB-CONFLICT-006 pilihan A yang sudah diputuskan 2026-09-16 tetapi amandemennya tidak pernah ditulis. Keduanya aditif, nol migration. SISA: collectedAt dipakai sebagai pembanding VAL-59 dan BELUM DISIMPAN — tiga pilihan penyimpanannya menunggu keputusan pemilik modul, lihat BE-LAB-37.md bagian 6. revision 14 — r14 (KOREKSI atas r13: tiga ruas tampil confirmedAt/confirmedByName/examinerDoctorName pada LabMonitoringItemResponse) disetujui pemilik modul 2026-09-16. r13 menyebut LabOrderListResponse, padahal ketiga menu pemeriksaan membaca grup Lab Monitoring; endpoint by-discipline yang menerima ruas r13 nol dipakai frontend. Aditif, nol migration. revision 13 — r13 (lima ruas respons konfirmasi: confirmedAt/confirmedByName/examinerDoctorName pada list, confirmedByUserId/examinerDoctorId pada detail) disetujui pemilik modul 2026-09-16; seluruhnya aditif, nol migration, menutup celah yang ditemukan BE-LAB-31 dan menahan FE-LAB-15 serta FE-LAB-17. revision 12 — r12 (POST /lab-orders/{id}/confirm + pembatalan wajib beralasan) disetujui pemilik modul 2026-09-15. revision 11 — r3..r9 approved; r10 (POST /lab-orders/by-examinations) disetujui pemilik modul 2026-09-15; r11 mencabut ruas clinicalNote sebelum sempat dibangun — LabOrder tidak punya kolom catatan, nol peminta di FE-LAB-14, nol dampak kode
  - LAB-STATE-v1: approved    # revision 3 — status Confirmed masuk antara Requested dan Accepted; pembatalan dipersempit. Disetujui 2026-09-15. revision 2 — tidak tersentuh amandemen 2026-09-14; tidak ada status baru
  - LAB-VAL-v1: approved      # revision 6 — VAL-70..VAL-75 disetujui pemilik modul 2026-09-15. revision 5 — VAL-01..VAL-63 approved (VAL-60 dicabut LAB-DEC-050); VAL-64..VAL-69 disetujui pemilik modul 2026-09-15
  - LAB-INT-v1: approved      # revision 3 — tidak tersentuh amandemen 2026-09-14; tidak ada integrasi baru
  - LAB-PERM-v1: approved     # revision 4 — resource LabSpecimenType disetujui 2026-09-14; tidak tersentuh pencabutan Quantity
contract_lock_scope: |
  TERKUNCI PENUH sejak 2026-09-02. LAB-OPEN-021 dijawab Muhammad Hamzah: entity baru milik
  Laboratorium memakai prefix Lab, sehingga kedua tabel batas nilai bernama LabValueBound dan
  LabValueOption. Tidak ada lagi bagian kontrak yang dikecualikan dari penguncian.
  Input hash sudah dihitung ulang 2026-09-02 sebagai sha256 penuh; lihat input_hashes dan input_hashes_method.

evidence_baseline:
  source: Analisis_Konsolidasi_Modul_Laboratorium.md
  adopted_by: LAB-DEC-025 .. LAB-DEC-031
  adopted_at: 2026-09-01
  authority_level: bukti sistem berjalan dan analis — tingkat 4-7, di bawah keputusan pemilik
  limitation: audio video belum ditranskripsi; aturan yang hanya disampaikan lisan belum tercakup

active_blockers:
  - LAB-RDY-C04     # bukti uji backend TIDAK dapat dijalankan ulang dari repository. /Tests/ dikecualikan .gitignore sejak fcabdff9 (2026-09-11), dan penelusuran berkas uji Laboratorium di backend menghasilkan 0. Akibatnya 70 baris SELESAI pada traceability bersandar pada bukti yang tidak dapat diperiksa siapa pun hari ini, termasuk CI. BUKAN bukti bahwa ujinya tidak pernah lulus; ia menjadi tidak dapat diperiksa. Keputusan tata kelola milik pemilik repository backend, di luar wewenang modul Laboratorium. DIAJUKAN 2026-09-17 lewat LAB-REQ-011. Verifikasi ulang 2026-09-17 memperjelas cakupannya: commit fcabdff9 oleh Sukma Giri Pratama pada 2026-09-11 menghapus 20 berkas dan 9.601 baris uji milik BEBERAPA modul, bukan hanya Laboratorium; .gitignore baris 378 memuat /Tests/; dan nol berkas uji terlacak git hari ini. Tiga kemungkinan jawaban ditawarkan, dan pilihan C - backend memang tidak memakai uji otomatis - dinyatakan sah asalkan diputuskan terbuka, karena seluruh task sejak BE-LAB-30 sudah membuktikan hasilnya lewat pemeriksaan sungguhan terhadap database
  - LAB-COORD-011   # diajukan LAB-REQ-007 bagian 4 pada 2026-09-16. Gerbang pengiriman pesan (WhatsApp) DAN pembangkit berkas PDF, keduanya nol pada platform. F7 diverifikasi ulang 2026-09-16 dan masih benar: nol Twilio/Fonnte/SendMessageAsync/IWhatsAppService/SmtpClient/MailKit di seluruh backend, nol pustaka PDF pada .csproj. QRCoder 1.8.0 sudah ada dan menutup barcode, tidak menutup PDF. Memblokir seluruh pengiriman hasil ke pasien (LAB-DEC-067). Milik platform
  - LAB-OPEN-029    # apakah persetujuan Profesor dan Dokter Lab yang disebut artifact Menu Hasil adalah tanda tangan klinis LAB-DEC-011, siapa pemegang wewenang Clinical Governance-nya, dan bagaimana keduanya menjadi peran pada LAB-PERM-v1. Bertaut LAB-SIGN-001. Diajukan LAB-REQ-007 bagian 3 pada 2026-09-16
  - LAB-OPEN-030    # jabatan Dokter Lantai nol kemunculan di blueprint; dipakai artifact bersama Dokter DPJP pada kolom Dokter Konfirmator angka kritis. Bertaut LAB-P0-004 dan LAB-OPEN-014. Diajukan LAB-REQ-007 bagian 5 pada 2026-09-16
  - LAB-OPEN-032    # ukuran cetak Nota Lab, Label Lab, dan Label Goldar. Artifact menandainya sendiri Confidence Medium dan menyebutnya default implementasi, bukan bukti; karena itu TIDAK diadopsi sebagai keputusan. Perlu profil printer dan media nyata. Tidak memblokir slice. Diajukan LAB-REQ-007 bagian 6 pada 2026-09-16
  - LAB-COORD-010   # Laboratorium memerlukan jalur baca status pembayaran milik Billing untuk satu kunjungan atau satu pesanan. Diturunkan LAB-DEC-062: layar lab mengunci tombol Proses Pemeriksaan bagi Mandiri/tunai sampai Lunas, tetapi Laboratorium nol menyimpan kolom pembayaran. Sampai jalur bacanya ada, penguncian tidak dapat ditegakkan backend. DIAJUKAN 2026-09-17 lewat LAB-REQ-008 kepada pemilik billing-kasir. Sebelum itu ia TIDAK PERNAH menjadi permintaan kepada siapa pun — LAB-REQ-007 bagian 7 keliru menyatakannya sudah diajukan, dan koreksinya ditulis pada dokumen itu. Temuan pemeriksaan 2026-09-17: Billing sudah punya GET .../invoices/encounters/{encounterId}/charge-summary, tetapi jawabannya memuat seluruh nominal yang justru tidak boleh dilihat Laboratorium (LAB-DEC-037), dan Status invoice (OPEN/FINAL/CLOSED/SETTLED_BY_WRITE_OFF) bukan status pembayaran. Yang diminta karena itu satu keputusan pemetaan + satu jalur baca sempit tanpa angka
  - LAB-OPEN-027    # apakah konfirmasi menjadi WAJIB sebelum Accepted. LAB-STATE-v1 r3 membiarkan jalur Requested -> Accepted tetap sah supaya pesanan yang sedang berjalan tidak berhenti dapat diproses. Menutupnya menuntut perlakuan atas pesanan in-flight dan kepastian setiap jalur pembuat pesanan melewati layar bertombol Konfirmasi. Tidak memblokir MVP-5c
  - LAB-COORD-006   # data induk instansi perujuk TIDAK punya endpoint tulis sama sekali; satu-satunya pengisinya LabDummyDataSeeder. Memblokir bagian pendaftaran rujukan pada menu Penerimaan Sampling/Specimen. Diajukan LAB-REQ-005 butir 1-3 pada 2026-09-14
  - LAB-COORD-007   # satu nilai EncounterPaymentType baru untuk piutang mitra, secara aditif, beserta penurunannya dari status PKS. Memblokir bagian metode pembayaran pada menu yang sama. Diajukan LAB-REQ-005 butir 4-7 pada 2026-09-14
  - LAB-OPEN-026b   # SISA dari LAB-OPEN-026, sifatnya BERUBAH dan itu yang penting: BUKAN lagi penahan. Selama SU-LAB-001 ber-IsAvailableForKiosk=false, layar kiosk yang menyaring daftar unitnya dengan ?isAvailableForKiosk=true tidak akan menampilkan Laboratorium — apakah layarnya memang menyaring begitu BELUM diperiksa, ia milik registration-management. Penandanya sendiri wewenang master-data. LAB-REQ-012 butir 4 tetap berlaku sebagai syarat agar pasien dapat memilih Laboratorium di kiosk, bukan sebagai penahan BE-EXT-05 yang sudah selesai
  - BE-EXT-05-BUKTI # SISA BUKTI, bukan sisa kode. Pembuktian empat cabang BE-EXT-05 dengan baris nyata — satu kunjungan yang harus tertutup dan TIGA yang harus tetap utuh (sudah punya LabOrder; bertujuan poliklinik; sudah CheckedInAt) — DITOLAK penjaga izin sesi 2026-09-17 karena QuilvianNewDevYoga database bersama. Nol baris disisipkan, nol jalan memutar ditempuh. Yang SUDAH terbukti adalah selektivitas penyaringnya, diukur tanpa mengubah satu baris pun: 0 sebagaimana ditulis / 14 tanpa klausa tujuan / 157 tanpa klausa kiosk yang 91 di antaranya WaitingForNurse. Yang BELUM terbukti adalah akibat tulisnya. Penyaringnya cocok nol baris hari ini sehingga risiko menjalankannya nol, tetapi itu tidak sama dengan terbukti
  - LAB-OPEN-024    # berapa lama usulan instansi perujuk boleh menggantung, apa yang terjadi bila ditolak, siapa yang berhak menggabungkan dua baris. Milik master-data; memblokir IMPLEMENTATION jalur penolakan usulan
  - LAB-SIGN-001    # tanda tangan klinis — memblokir S4, S4b, S4c, S5, S6. SATU-SATUNYA penahan kelimanya: LAB-COORD-001 dan LAB-COORD-002 sudah ditutup 2026-09-01. Permintaan LAB-REQ-004 diajukan 2026-09-09, menunggu jawaban. HAMBATAN SEBELUM HAMBATAN, diajukan 2026-09-17 lewat LAB-REQ-009: LAB-REQ-004 ditujukan kepada pemegang wewenang Clinical Governance, dan jabatan itu belum pernah diisi — owners.clinical_governance masih "belum ditetapkan", sehingga dokumen itu tidak punya tujuan yang sah dan menggantung delapan hari bukan karena isinya. LAB-REQ-009 memohon penetapannya kepada manajemen rumah sakit, dan ia prasyarat LAB-REQ-004 maupun LAB-REQ-007 bagian 3 dan 5
  - LAB-AMD-001     # amandemen rawat-jalan — memblokir S1b
  - LAB-OPEN-018b   # SISA: marketplace quilvian masih terdaftar ke MHamzah1/QuilvianEngineeringSkillsClaude. Rules root runtime sudah lengkap lewat penyegaran manual, tetapi /plugin update berikutnya akan mengembalikannya ke 13 berkas. Perbaikan tetap: daftarkan ulang marketplace ke DevBenari/QuilvianEngineeringSkills. TIDAK memblokir implementasi saat ini

  - LAB-OPEN-012    # jumlah data lab existing belum diverifikasi — prasyarat migration BE-LAB-11. Tidak menahan BE-LAB-20, yang migration-nya sudah diterapkan 2026-09-14
  - LAB-OPEN-013    # dampak cito dan duplo pada tarif
  - LAB-OPEN-014    # nilai kritis untuk mikrobiologi dan patologi anatomi
  - LAB-OPEN-017    # makna penanda Definitif
  - LAB-P0-001      # matriks kewenangan per peran
  - LAB-P0-002      # urutan status resmi, termasuk Confirmed
  - LAB-P0-003      # aturan pembatalan dan koreksi
  - LAB-P0-004      # alur nilai kritis lengkap
  - LAB-P0-005      # integrasi alat laboratorium
  - LAB-P0-006      # kebijakan jejak audit
  - LAB-P0-007      # aturan tagihan dan cakupan
  - LAB-P0-008      # penyelarasan antaraplikasi

closed_blockers:                # ditutup 2026-09-02 kecuali yang bertanggal lain, disimpan sebagai jejak
  - LAB-RDY-C02     # ditutup 2026-09-16 oleh LAB-DEC-074 dan FE-LAB-18. Aturan tanggal dinyatakan berlaku module-wide; perbaikannya aditif — prop OPSIONAL max pada FilterDatePicker dengan bawaan tidak berubah, sehingga 132 pemakai lain nol terdampak dan itu dibuktikan terbalik lewat tiga asersi. Bukti: 999/999 uji unit lulus, build produksi lulus, lint nol tambahan. Batas jujur: nol verifikasi manual pada aplikasi berjalan
  - LAB-RDY-C01     # ditutup 2026-09-16, kondisi pertama LAB-RDY-001. Kedua *_commit_sha disegarkan ke 13665452 dan 686038858 sesudah audit kesiapan selesai — bukan saat audit berjalan, supaya penilai tidak menyunting masukannya sendiri
  - LAB-CONFLICT-009 # ditutup 2026-09-16 oleh LAB-DEC-070 — blueprint bertahan. Menu Hasil mengikuti pola tiga daftar sejajar per disiplin; disiplin ditentukan jalur yang dipanggil. Usulan artifact menyatukan ketiganya dalam satu datatable TIDAK diadopsi; alasan LAB-DEC-025 masih berlaku. LabMonitoringQuery dipakai ulang, nol ruas disiplin ditambahkan
  - LAB-OPEN-028    # ditutup 2026-09-16 oleh LAB-DEC-071 — pembanding rentang tanggal inklusif, Tgl Awal <= Tgl Akhir. Mengoreksi RULE-004 artifact, dan nol mengubah perilaku source: penyaring yang berjalan sudah inklusif pada kedua ujung
  - LAB-OPEN-033    # ditutup 2026-09-16 oleh LAB-DEC-072 — LabOrder memperoleh kolom nomor order berupa nomor urut terbaca, pola PatientEncounterNumberService. PEKERJAANNYA SELESAI 2026-09-17 lewat BE-LAB-36: kolom OrderNumber NOT NULL dan unik, layanan alokasi, index unik, dan migration bersunting tangan, seluruhnya diterapkan ke QuilvianNewDevYoga. Peringatan pola acuan DIPATUHI: nol baris dimuat ke memori, celah tidak pernah diisi ulang. Dua temuan perancangan tambahan: alokasi wajib berblok pada jalur by-examinations, dan kunci advisory nol menjaga di luar transaksi eksplisit. SISA: nomornya belum dapat dibaca di luar backend — usul LAB-API-v1 r16 menunggu persetujuan pemilik modul
  - LAB-OPEN-031    # ditutup 2026-09-16 oleh LAB-DEC-073 — menu Hasil hanya memuat pesanan Laboratorium. Nilai Unit Layanan Radiologi terbawa dari tabel unit layanan bersama dan tidak akan pernah muncul sebagai baris. Nol koordinasi dengan Radiologi yang perlu dibuka
  - LAB-CONFLICT-008 # ditutup 2026-09-15 oleh LAB-DEC-060 sampai LAB-DEC-063 — rekonsiliasi bukti putaran 2 RECONCILED. Siklus hidup wadah bertahan; status Confirmed dan dokter pemeriksa diadopsi; alasan pembatalan wajib disimpan backend; pembayaran mengunci tombol Proses tetapi dibaca dari Billing
  - LAB-CONFLICT-007 # ditutup 2026-09-15 oleh BE-LAB-29 — Discipline diturunkan dari katalog pada satu-satunya jalur tulis LabOrder; jalur itu dibuktikan tunggal lewat penelusuran new LabOrder dan LabOrders.Add di seluruh aplikasi. Katalog 10 dari 10 tergolong. Dua baris data lama sengaja tidak diperbaiki — perubahan data, wewenang terpisah
  - LAB-COORD-008   # ditutup 2026-09-15 oleh LAB-REQ-006 — Andry Zain menyetujui kiosk bertambah bagian Laboratorium, secara aditif
  - LAB-COORD-009   # ditutup 2026-09-15 oleh LAB-REQ-006 dan LAB-DEC-058 — kunjungan kiosk yang tidak dilanjutkan ditutup akhir hari layanan, biaya pendaftaran gugur
  - LAB-OPEN-022    # ditutup 2026-09-14 oleh impact scan capability map revision 3
  - LAB-OPEN-023    # ditutup 2026-09-14: input_revisions, input_hashes, dan kedua commit_sha pada manifest ini disinkronkan
  - LAB-CONFLICT-003 # ditutup 2026-09-14 oleh LAB-DEC-046 dan LAB-DEC-047
  - LAB-OPEN-018    # rules root runtime kini memuat 32 berkas termasuk GLOBAL_RULES.md dan backend/engineering/. Disegarkan dari sumber canonical DevBenari/QuilvianEngineeringSkills atas persetujuan pilihan B. Gerbang AGENTS.md tidak lagi aktif. Sisa pekerjaan dicatat sebagai LAB-OPEN-018b
  - LAB-OPEN-019    # lifecycle registry PLANNED -> ACTIVE, disetujui Muhammad Hamzah lewat LAB-REQ-002; diterapkan pada registry canonical dan salinan docs/engineering/
  - LAB-OPEN-020    # Invoke-QbeConformanceCheck.ps1 diperbaiki atas persetujuan Andry Zain: empat rujukan agents/rules/engineering/ diganti docs/engineering/. Checker kini PASS, exit 0
  - LAB-OPEN-021    # prefix data induk ditetapkan Lab; LabValueBound dan LabValueOption. MstLabRejectionReason tetap legacy

cross_module_approvals_pending:   # dibuka 2026-09-14
  request_id: LAB-REQ-005
  file: approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md
  addressed_to: pemilik master-data; pemilik registration-management; pemilik billing-kasir
  status: menunggu jawaban
  scope: LAB-COORD-006 (butir 1-3); LAB-COORD-007 (butir 4-7); konfirmasi perlakuan PaymentType per jalur; LAB-OPEN-024
  note: penerima belum ditetapkan — blueprint belum mencatat nama pemilik ketiga modul itu

cross_module_approvals:
  request_id: LAB-REQ-001
  approved_by: andryzainhome <andryzain01@gmail.com>; sukmagp — Sukma Giri Pratama <sukmagiri11@gmail.com>
  approved_at: 2026-09-01
  scope: LAB-COORD-001 .. LAB-COORD-005
  not_covered: LAB-OPEN-012 memerlukan jawaban faktual; LAB-SIGN-001 memerlukan wewenang klinis. LAB-OPEN-002 ditutup 2026-09-01 oleh LAB-FACT-007, menurunkan LAB-OPEN-018 dan LAB-OPEN-019. Pemeriksaan 2026-09-02 menurunkan LAB-OPEN-020 (checker QBE) dan memisahkan LAB-OPEN-021 (prefix) dari LAB-OPEN-018

inherited_decisions:
  source: rawat-jalan / RJ-BIL-GATE-DEC-003
  status: locked-draft, tata kelola formal OPEN
  ids: [LAB-INH-001 .. LAB-INH-013]
```

## Daftar artefak

| Berkas | Ditulis oleh | Status |
|---|---|---|
| `blueprint-manifest.md` | `design-business-module` | rev 25 — `LAB-REQ-004` didaftarkan, `LAB-SIGN-001` dicatat sebagai satu-satunya penahan lima slice |
| `00-interview-decisions.md` | `grill-me` | rev 22 — `LAB-SIGN-001` diajukan lewat `LAB-REQ-004`; **36 keputusan `approved`**; 5 koordinasi lintas modul ditutup |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | rev 2 — impact scan 2026-09-02, `STALE` dicabut, tidak ada status kemampuan yang berubah |
| `02-requirement-completeness-assessment.md` | `requirement-completeness-gate` | rev 5 — `PARTIALLY_READY`, 10 dari 21 bagian siap |
| `03-domain-architecture.md` | `hospital-domain-architect` | rev 4 — **`DOMAIN_ARCHITECTURE_READY`** untuk 10 slice |
| `05-evidence-reconciliation.md` | `design-business-module` | rev 4 — `PARTIALLY_RECONCILED`. Putaran 1 dan 2 `RECONCILED`; putaran 3 (Menu Hasil) menutup 3 dari 5 pertentangan, dua sisanya bukan wewenang pemilik modul |
| `06-readiness-assessment.md` | `verify-module-readiness` | rev 3 — **`READY_WITH_CONDITIONS`** untuk 10 slice MVP Rilis 1, skor tertimbang **7,7/10**. `C-01` dan `C-02` **ditutup 2026-09-16**; dua tersisa, **keduanya di luar wewenang modul**: `C-03` dua jalur integrasi belum ada, `C-04` bukti uji backend tidak dapat dijalankan ulang |
| `02-backend-architecture.md` | `design-business-module` | rev 3 |
| `03-frontend-architecture.md` | `design-business-module` | rev 3 — menu data induk mengikuti konvensi FE yang sudah ada |
| `04-prd-to-mvp.md` | `design-business-module` | rev 3 — batas MVP diperluas, 10 epic, 5 gelombang; penahan `S5` dan `S6` dikoreksi 2026-09-09 |
| `erd/00-context-erd.md` | `design-business-module` | rev 2 — data induk perujuk dan kolom disiplin masuk |
| `erd/laboratory-operations.md` | `design-business-module` | rev 2 — lapis rujukan katalog, harga, cakupan |
| `erd/data-dictionary.md` | `design-business-module` | rev 2 — empat tabel milik modul lain didokumentasikan |
| `contracts/api-contract.md` | `design-business-module` | rev 6 — `approved`, dikunci 2026-09-02; 3 grup endpoint baru pada r3, sepuluh endpoint baca pada r4, paging `GET /lab-orders` pada r5, `GET /lab-rejection-reasons/{id}` pada r6. Status 16 endpoint dikoreksi 2026-09-08 |
| `contracts/state-transition-matrix.md` | `design-business-module` | rev 2 — `approved`, dikunci 2026-09-02; sudah disesuaikan `LAB-DEC-026` |
| `contracts/validation-matrix.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; VAL-40 sampai VAL-50 |
| `contracts/integration-contract.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; `INT-05` dan `INT-06` |
| `contracts/permission-audit-matrix.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; 9 kewenangan baru |
| `testing/acceptance-test-matrix.md` | `design-business-module` | rev 3 — dua coverage gap ditutup: AC-11 alur lintas unit dan AC-19 batas reagen |
| `roadmap/backend-roadmap.md` | `plan-module-delivery` | rev 3 — `DRAFT`, 16 task Laboratorium + 3 task eksternal; lima dimensi cakupan diaudit |
| `roadmap/frontend-roadmap.md` | `plan-module-delivery` | rev 1 — `DRAFT`, 9 task, dipasangkan ke gelombang backendnya |
| `roadmap/traceability.md` | `plan-module-delivery` | rev 5 — `DRAFT`, tujuh dimensi cakupan terpetakan penuh |

### Artefak operasional — di luar daftar hash

| Berkas | Sifat | Status |
|---|---|---|
| `approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` | Operasional, bukan artefak desain | **`dijawab sebagian`** — 6 selesai, 5 terbuka. Butir 9, 10, dan 11 diajukan 2026-09-02 |
| `approval-requests/2026-09-02-permintaan-muhammad-hamzah.md` | Operasional | `LAB-REQ-002` — 3 butir terarah: rules root, lifecycle registry, prefix data induk |
| `approval-requests/2026-09-02-permintaan-master-data-dan-registrasi.md` | Operasional | `LAB-REQ-003` — `BE-EXT-01` sampai `BE-EXT-03`, izinnya sudah ada sejak 2026-09-01 |
| `approval-requests/2026-09-09-permintaan-tanda-tangan-klinis.md` | Operasional | `LAB-REQ-004` — **`terbuka`**. Menutup `LAB-SIGN-001`: tiga keputusan untuk ditandatangani, dua penetapan rumah sakit, tiga pertanyaan klinis. Penanda tangannya sendiri belum ditetapkan — **prasyaratnya `LAB-REQ-009`** |
| `approval-requests/2026-09-16-permintaan-menu-hasil-dan-pengiriman-hasil.md` | Operasional | `LAB-REQ-007` — **`terbuka`**. Empat bagian: persetujuan klinis (`LAB-OPEN-029`), gerbang WhatsApp dan pembangkit PDF (`LAB-COORD-011`), jabatan `Dokter Lantai` (`LAB-OPEN-030`), ukuran cetak (`LAB-OPEN-032`). **Bagian 7 dikoreksi 2026-09-17** — klaim bahwa `LAB-COORD-010` sudah diajukan terbukti keliru |
| `approval-requests/2026-09-17-permintaan-jalur-baca-status-pembayaran.md` | Operasional | `LAB-REQ-008` — **`terbuka`**. Menutup `LAB-COORD-010`. Dua butir: keputusan pemetaan keadaan tagihan menjadi tiga kata layar, dan satu jalur baca sempit **tanpa nominal**. Ditujukan kepada pemilik `billing-kasir` |
| `approval-requests/2026-09-17-nota-penetapan-wewenang-clinical-governance.md` | Operasional | `LAB-REQ-009` — **`terbuka`**. Memohon **satu nama** kepada manajemen rumah sakit. **Prasyarat `LAB-REQ-004` dan `LAB-REQ-007` bagian 3 dan 5**; tanpanya ketiganya tidak punya tujuan yang sah |
| `approval-requests/2026-09-17-permintaan-kunjungan-dari-kiosk.md` | Operasional | `LAB-REQ-012` — **butir 3 `terjawab` 2026-09-17** (pilihan **A**, pemilik `registration-management` Andry Zain), **butir 4 tetap `terbuka`** bagi `master-data`. **Kedua penahannya kemudian terbantah oleh pemeriksaan source, bukan sekadar dijawab:** `LAB-OPEN-025` menyebut `EncounterIntakeService` yang bukan jalur kiosk — `POST /patient-encounters/kiosk` sudah ada, dijaga hanya `KioskReadPolicy`, nol memikul `[AccessPermission]`, sehingga **pilihan A sudah berdiri di source sejak sebelum diusulkan**; `LAB-OPEN-026` menyebut `IsAvailableForKiosk` yang nol dibaca jalur pembentukan kunjungan, sedangkan `IsAvailableForRegistration` pada `SU-LAB-001` bernilai `true`. Keduanya **ditutup**, dan sisanya dicatat sebagai `LAB-OPEN-026b`. `BE-EXT-05` selesai; lihat `task/report/backend/BE-EXT-05.md` |
| `approval-requests/2026-09-17-permintaan-keterlacakan-bukti-uji-backend.md` | Operasional | `LAB-REQ-011` — **`terbuka`**. Menutup `LAB-RDY-C04`. Satu keputusan tata kelola: bagaimana bukti uji backend tetap dapat diperiksa sesudah `fcabdff9` mengeluarkan seluruh berkas uji. **Akibatnya melintasi beberapa modul**, bukan hanya Laboratorium. Ditujukan kepada pemilik repository backend |
| `approval-requests/2026-09-17-permintaan-satuan-ukur-laboratorium.md` | Operasional | `LAB-REQ-010` — **`terbuka`**. Menutup `DATA-MST-MEASUREMENT`. Tiga baris satuan: `µL`, `blok`, `slide`. **Penahan kedua yang ternyata belum pernah diajukan kepada siapa pun** sejak dicatat 2026-09-15 — pola yang sama dengan `LAB-COORD-010`. Ditujukan kepada pemilik `master-data` |

## Catatan status

Blueprint **disetujui** pemilik modul pada 2026-09-01. Seluruh artefak desain tetap berstatus
`draft` karena cakupan berubah setelah bukti lapangan diadopsi. Tidak ada satu baris source
aplikasi yang diubah pada tahap ini.

### Artefak yang masih perlu disesuaikan

| Berkas | Yang belum masuk | Keadaan |
|---|---|---|
| `erd/00-context-erd.md` | Instansi dan dokter perujuk sebagai data induk milik `BC-MD`; kolom disiplin pada `MstProcedure` | ✅ **Selesai** — diverifikasi 2026-09-02: `MstReferralInstitution`, `MstReferralDoctor`, dan `LabDiscipline` sudah masuk beserta relasinya |
| `erd/laboratory-operations.md` | Relasi ke data induk perujuk | ✅ **Selesai** — diverifikasi 2026-09-02 |
| `erd/data-dictionary.md` | Kolom kunci data induk perujuk; kolom disiplin `MstProcedure` | ✅ **Selesai** — diverifikasi 2026-09-02, termasuk DDL bagian 9b |
| `contracts/state-transition-matrix.md` | Perpindahan status pendaftaran | Tidak ada lifecycle baru milik Laboratorium; kunjungan mengikuti lifecycle Registrasi |

### Yang menahan penerbitan roadmap — per 2026-09-02

`LAB-OPEN-019` **tidak** menahan roadmap; ia menahan implementasi. Ketiga penahan penerbitan
roadmap sudah dibereskan pada 2026-09-02:

| # | Penahan | Keadaan |
|---|---|---|
| ~~1~~ | ~~Kelima kontrak masih `draft`~~ | ✅ **Selesai.** Kelimanya dikunci `approved` oleh pemilik modul, diberi `Revision`, `approved_by`/`approved_at`, dan `Backend SHA` |
| ~~2~~ | ~~Kontrak tertinggal dari keputusan~~ | ✅ **Selesai.** `Input revision` diselaraskan ke `Decisions rev 20`; daftar artefak dikoreksi dari rev 18 ke rev 20. Isi kontrak diverifikasi sudah memuat perubahan revision 18, 19, dan 20 — tidak ada rujukan `Trx*` usang yang tersisa |
| ~~3~~ | ~~`capability_map` berstatus `STALE`~~ | ✅ **Selesai.** Impact scan dijalankan, `CAP-11` dan bagian utang teknis diverifikasi tidak berubah, `STALE` dicabut. Peta naik ke revision 2 |

**Roadmap sudah diterbitkan 2026-09-02** di `roadmap/` — `backend-roadmap.md`,
`frontend-roadmap.md`, dan `traceability.md`, seluruhnya revision 1 berstatus `DRAFT`. Ketentuan
yang berlaku padanya:

| Ketentuan | Alasan |
|---|---|
| Task yang membuat entity `Lab*` bertanda `BLOCKED` | `LAB-OPEN-019` — lifecycle registry masih `PLANNED` |
| Task yang membuat dua tabel batas nilai bertanda `BLOCKED` | `LAB-OPEN-021` — penamaan `Mst` atau `Lab` belum ditetapkan |
| Task implementasi backend mana pun tidak boleh dieksekusi | `LAB-OPEN-018` — rules root runtime belum lengkap; `AGENTS.md` memaksa `BLOCKED — canonical governance unavailable` |
| Migration pemisahan wadah tetap tertahan | `LAB-OPEN-012` — jumlah baris `TrxLabSpecimen` produksi belum diketahui |

### Utang pembukuan — ✅ seluruhnya ditutup 2026-09-02

| Butir | Keadaan |
|---|---|
| ~~`input_hashes` masih milik revisi lama~~ | ✅ **Ditutup.** Konvensinya bukan hash 16 digit melainkan **sha256 penuh atas isi ber-line-ending LF**, ditemukan dari `pharmacy` dan `billing-kasir`. Metodenya diuji lebih dulu terhadap keempat `artifact_hashes` pharmacy dan cocok persis, baru dipakai. Keempat hash Laboratorium dihitung ulang dan diverifikasi ulang setelah seluruh suntingan selesai |
| ~~`Riwayat Revisi` pada `00-interview-decisions.md` memuat baris revision 19 dua kali~~ | ✅ **Ditutup.** Kedua salinan digabungkan; keduanya sempat bertentangan soal lokasi canonical dan soal arti `LAB-OPEN-018`. Dicatat sebagai decisions revision 21 |

**Cara memeriksa ulang hash kapan pun:**

```bash
tr -d '\r' < 00-interview-decisions.md | sha256sum
```

Hasilnya wajib sama dengan nilai pada `input_hashes`. Bila berbeda, berkas masukan berubah dan
seluruh artefak turunannya — termasuk kontrak dan roadmap — menjadi stale.

Nomor awalan berkas sengaja berulang — `02-` dan `03-` masing-masing dipakai dua kali. Ini
mengikuti pola yang sudah dipakai modul `pharmacy` dan `operations`, di mana artefak gerbang
dan artefak desain hidup berdampingan.

## Pemicu perubahan revision

| Pemicu | Akibat |
|---|---|
| Backend bergerak dari `c87d9c0` | Impact scan `trace-existing-capabilities`; manifest naik revision |
| Frontend bergerak dari `688daff90` | Impact scan bagian frontend |
| Salah satu blocker aktif ditutup | Slice terkait masuk scope; seluruh artefak desain naik revision |
| `LAB-DEC-024` berubah | Arsitektur backend, ERD, dan kontrak wajib disusun ulang |
