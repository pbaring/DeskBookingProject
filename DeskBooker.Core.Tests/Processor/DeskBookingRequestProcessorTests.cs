using DeskBooker.Core.Domain;
using DeskBooker.Core.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepository;
        private readonly Mock<IDeskRepository> _deskRepository;

        public DeskBookingRequestProcessorTests()
        {
            _request = new DeskBookingRequest()
            {
                FirstName = "Pinky",
                LastName = "Baring",
                Email = "pinky.baring@customer.com",
                Date = DateTime.Now
            };
            _availableDesks = new List<Desk> { new Desk {Id = 7} };
            //mock
            //dependencies
            _deskBookingRepository = new Mock<IDeskBookingRepository>();
            _deskRepository = new Mock<IDeskRepository>();
            _deskRepository.Setup(x => x.GetAvailableDesks(_request.Date)).Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskBookingRepository.Object, _deskRepository.Object);
        }

        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            //Act
            DeskBookingResult result = _processor.BookDesk(_request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_request.FirstName, result.FirstName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));

            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            //Arrange
            DeskBooking savedDeskBooking = null;

            //call dependencies
            //Act
            _deskBookingRepository.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                {
                    savedDeskBooking = deskBooking;
                });

            _processor.BookDesk(_request);

            //Assert
            _deskBookingRepository.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.NotNull(savedDeskBooking);
            Assert.Equal(_request.FirstName, savedDeskBooking.FirstName);
            Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            Assert.Equal(_request.Email, savedDeskBooking.Email);
            Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, savedDeskBooking.DeskId    );
        }

        [Fact]
        public void ShouldNotSaveDeskBookingIfDeskIsNotAvailable()
        {
            _availableDesks.Clear();
            _processor.BookDesk(_request);
            //Assert
            _deskBookingRepository.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);
        }

        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedDeskBookingResultCode(
            DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            //arrange
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }

            //act
            var result = _processor.BookDesk(_request);

            //assert
            Assert.Equal(expectedResultCode, result.Code);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(
            int? expectedResultId, bool isDeskAvailable)
        {
            //arrange
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                //call dependencies
                //Act
                _deskBookingRepository.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(deskBooking =>
                    {
                        deskBooking.Id = expectedResultId.Value;
                    });
            }

            //act
            var result = _processor.BookDesk(_request);

            //assert
            Assert.Equal(expectedResultId, result.DeskBookingId);
        }
    }
}
